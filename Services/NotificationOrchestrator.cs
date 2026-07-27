using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using sms.api.Data;
using sms.api.Hubs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Services;

public interface INotificationOrchestrator
{
    Task<NotificationDispatchResult> NotifyAsync(NotificationRequest request, CancellationToken ct = default);
}

public class NotificationOrchestrator : INotificationOrchestrator
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBackgroundTaskQueue _queue;
    private readonly NotificationSettings _settings;
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly ILogger<NotificationOrchestrator> _logger;

    public NotificationOrchestrator(
        IServiceScopeFactory scopeFactory,
        IBackgroundTaskQueue queue,
        IOptions<NotificationSettings> settings,
        IHubContext<NotificationHub> notificationHub,
        ILogger<NotificationOrchestrator> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _settings = settings.Value;
        _notificationHub = notificationHub;
        _logger = logger;
    }

    public async Task<NotificationDispatchResult> NotifyAsync(NotificationRequest request, CancellationToken ct = default)
    {
        var result = new NotificationDispatchResult();
        if (request == null || string.IsNullOrWhiteSpace(request.Title))
        {
            return result;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var userIds = new HashSet<Guid>(request.UserIds ?? new List<Guid>());

        if (request.IncludeAdmins)
        {
            foreach (var idStr in _settings.AdminUserIds ?? new List<string>())
            {
                if (Guid.TryParse(idStr, out var id)) userIds.Add(id);
            }

            // Always include active Admin-role users (not only when AdminUserIds is empty)
            var adminIds = await db.Users
                .AsNoTracking()
                .Where(u => u.IsActive && (u.Role == "Admin" || u.Role == "admin"))
                .Select(u => u.Id)
                .Take(50)
                .ToListAsync(ct);
            foreach (var id in adminIds) userIds.Add(id);

            // Also map AdminEmails → user accounts so inbox matches who got the email
            var adminEmails = (_settings.AdminEmails ?? new List<string>())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLower())
                .Distinct()
                .ToList();
            if (adminEmails.Count > 0)
            {
                var byEmail = await db.Users
                    .AsNoTracking()
                    .Where(u => u.IsActive && u.Email != null && adminEmails.Contains(u.Email.ToLower()))
                    .Select(u => u.Id)
                    .Take(50)
                    .ToListAsync(ct);
                foreach (var id in byEmail) userIds.Add(id);
            }
        }

        result.RecipientCount = userIds.Count;

        var channels = request.Channels;
        if (!_settings.EnableInApp) channels &= ~NotificationChannels.InApp;
        if (!_settings.EnablePush) channels &= ~NotificationChannels.Push;
        if (!_settings.EnableEmail) channels &= ~NotificationChannels.Email;
        if (!_settings.EnableSms) channels &= ~NotificationChannels.Sms;

        var dataJson = request.Data != null ? JsonSerializer.Serialize(request.Data) : null;
        var notificationIds = new Dictionary<Guid, Guid>();
        var realtimePayloads = new List<(Guid UserId, object Payload)>();

        foreach (var userId in userIds)
        {
            if (!string.IsNullOrEmpty(request.IdempotencyKey))
            {
                var since = DateTime.UtcNow.AddHours(-6);
                var dup = await db.NotificationDeliveryLogs.AnyAsync(l =>
                    l.UserId == userId
                    && l.Type == request.Type
                    && l.ProviderMessageId == request.IdempotencyKey
                    && l.CreatedAt >= since, ct);
                if (dup) continue;
            }

            var prefs = await GetOrDefaultPrefsAsync(db, userId, ct);
            if (IsMuted(prefs, request.Category)) continue;

            Guid? inboxId = null;
            if (channels.HasFlag(NotificationChannels.InApp) && prefs.EnableInApp)
            {
                var row = new InAppNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = request.Title,
                    Body = request.Body,
                    Type = request.Type,
                    EntityType = request.EntityType,
                    EntityId = request.EntityId,
                    Severity = request.Severity,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    DataJson = dataJson
                };
                db.InAppNotifications.Add(row);
                inboxId = row.Id;
                notificationIds[userId] = row.Id;
                result.InAppCreated++;

                realtimePayloads.Add((userId, new
                {
                    id = row.Id,
                    title = row.Title,
                    body = row.Body,
                    type = row.Type,
                    entityType = row.EntityType,
                    entityId = row.EntityId,
                    severity = row.Severity,
                    isRead = false,
                    createdAt = row.CreatedAt,
                    dataJson = row.DataJson
                }));

                await LogAsync(db, inboxId, userId, "InApp", "Sent", null, null, request.Type, request.IdempotencyKey);
            }
        }

        await db.SaveChangesAsync(ct);

        // Real-time inbox for connected clients (SignalR)
        foreach (var (userId, payload) in realtimePayloads)
        {
            try
            {
                await _notificationHub.Clients.User(userId.ToString())
                    .SendAsync("InboxNotification", payload, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SignalR inbox push failed for {UserId}", userId);
            }
        }

        // Extra admin emails/phones (not tied to user accounts)
        var extraEmails = request.IncludeAdmins
            ? (_settings.AdminEmails ?? new List<string>()).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            : new List<string>();
        var extraPhones = request.IncludeAdmins
            ? (_settings.AdminPhones ?? new List<string>()).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList()
            : new List<string>();

        if (channels.HasFlag(NotificationChannels.Push)
            || channels.HasFlag(NotificationChannels.Email)
            || channels.HasFlag(NotificationChannels.Sms))
        {
            var snapshot = new DeliverySnapshot
            {
                UserIds = userIds.ToList(),
                NotificationIds = notificationIds,
                Title = request.Title,
                Body = request.Body,
                SmsBody = string.IsNullOrWhiteSpace(request.SmsBody) ? Truncate(request.Body, 160) : request.SmsBody!,
                EmailSubject = request.EmailSubject ?? request.Title,
                EmailHtmlBody = request.EmailHtmlBody ?? $"<p>{System.Net.WebUtility.HtmlEncode(request.Body)}</p>",
                Type = request.Type,
                Category = request.Category,
                Channels = channels,
                Data = request.Data,
                ExtraEmails = extraEmails,
                ExtraPhones = extraPhones,
                IdempotencyKey = request.IdempotencyKey
            };

            if (channels.HasFlag(NotificationChannels.Push)) result.PushQueued = userIds.Count;
            if (channels.HasFlag(NotificationChannels.Email)) result.EmailQueued = userIds.Count + extraEmails.Count;
            if (channels.HasFlag(NotificationChannels.Sms)) result.SmsQueued = userIds.Count + extraPhones.Count;

            _queue.Enqueue(ct2 => DeliverChannelsAsync(snapshot, ct2));
        }
        else if (extraEmails.Count > 0 || extraPhones.Count > 0)
        {
            // Admins-only email/SMS without user ids
            var snapshot = new DeliverySnapshot
            {
                UserIds = new List<Guid>(),
                NotificationIds = new Dictionary<Guid, Guid>(),
                Title = request.Title,
                Body = request.Body,
                SmsBody = string.IsNullOrWhiteSpace(request.SmsBody) ? Truncate(request.Body, 160) : request.SmsBody!,
                EmailSubject = request.EmailSubject ?? request.Title,
                EmailHtmlBody = request.EmailHtmlBody ?? $"<p>{System.Net.WebUtility.HtmlEncode(request.Body)}</p>",
                Type = request.Type,
                Category = request.Category,
                Channels = channels,
                Data = request.Data,
                ExtraEmails = extraEmails,
                ExtraPhones = extraPhones,
                IdempotencyKey = request.IdempotencyKey
            };
            _queue.Enqueue(ct2 => DeliverChannelsAsync(snapshot, ct2));
        }

        return result;
    }

    private async Task DeliverChannelsAsync(DeliverySnapshot snap, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var firebase = scope.ServiceProvider.GetRequiredService<IFirebaseNotificationService>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var sms = scope.ServiceProvider.GetRequiredService<ITwilioSmsService>();

        foreach (var userId in snap.UserIds)
        {
            var prefs = await GetOrDefaultPrefsAsync(db, userId, ct);
            if (IsMuted(prefs, snap.Category)) continue;

            snap.NotificationIds.TryGetValue(userId, out var notifId);
            Guid? nId = notifId == Guid.Empty ? null : notifId;

            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (snap.Channels.HasFlag(NotificationChannels.Push) && prefs.EnablePush)
            {
                try
                {
                    var pushResult = await firebase.SendNotificationToUserWithStatusAsync(
                        userId, snap.Title, snap.Body, snap.Data);
                    var ok = pushResult.SuccessTokenCount > 0;
                    await LogAsync(db, nId, userId, "Push",
                        ok ? "Sent" : (pushResult.UsersWithoutActiveTokens > 0 ? "Skipped" : "Failed"),
                        null,
                        ok ? null : "No active tokens or send failed",
                        snap.Type, snap.IdempotencyKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Push failed for {UserId}", userId);
                    await LogAsync(db, nId, userId, "Push", "Failed", null, ex.Message, snap.Type, snap.IdempotencyKey);
                }
            }

            if (snap.Channels.HasFlag(NotificationChannels.Email) && prefs.EnableEmail && user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    await email.SendAsync(user.Email, snap.EmailSubject, snap.EmailHtmlBody, ct);
                    await LogAsync(db, nId, userId, "Email", "Sent", null, null, snap.Type, snap.IdempotencyKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email failed for {UserId}", userId);
                    await LogAsync(db, nId, userId, "Email", "Failed", null, ex.Message, snap.Type, snap.IdempotencyKey);
                }
            }

            if (snap.Channels.HasFlag(NotificationChannels.Sms) && prefs.EnableSms)
            {
                var phone = await ResolvePhoneAsync(db, userId, user, ct);
                if (!string.IsNullOrEmpty(phone))
                {
                    var smsResult = await sms.SendAsync(phone, snap.SmsBody, ct);
                    await LogAsync(db, nId, userId, "Sms",
                        smsResult.Skipped ? "Skipped" : (smsResult.Success ? "Sent" : "Failed"),
                        smsResult.MessageId, smsResult.Error, snap.Type, snap.IdempotencyKey);
                }
                else
                {
                    await LogAsync(db, nId, userId, "Sms", "Skipped", null, "No phone", snap.Type, snap.IdempotencyKey);
                }
            }
        }

        if (snap.Channels.HasFlag(NotificationChannels.Email))
        {
            foreach (var addr in snap.ExtraEmails)
            {
                try
                {
                    await email.SendAsync(addr, snap.EmailSubject, snap.EmailHtmlBody, ct);
                    await LogAsync(db, null, null, "Email", "Sent", null, null, snap.Type, snap.IdempotencyKey);
                }
                catch (Exception ex)
                {
                    await LogAsync(db, null, null, "Email", "Failed", null, ex.Message, snap.Type, snap.IdempotencyKey);
                }
            }
        }

        if (snap.Channels.HasFlag(NotificationChannels.Sms))
        {
            foreach (var phone in snap.ExtraPhones)
            {
                var smsResult = await sms.SendAsync(phone, snap.SmsBody, ct);
                await LogAsync(db, null, null, "Sms",
                    smsResult.Skipped ? "Skipped" : (smsResult.Success ? "Sent" : "Failed"),
                    smsResult.MessageId, smsResult.Error, snap.Type, snap.IdempotencyKey);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task<string?> ResolvePhoneAsync(ApplicationDbContext db, Guid userId, User? user, CancellationToken ct)
    {
        if (user != null && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            return user.PhoneNumber;

        var student = await db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (student != null)
        {
            if (!string.IsNullOrWhiteSpace(student.ParentPhone)) return student.ParentPhone;
            if (!string.IsNullOrWhiteSpace(student.GuardianContact)) return student.GuardianContact;
            if (!string.IsNullOrWhiteSpace(student.PhoneNumber)) return student.PhoneNumber;
        }

        var staff = await db.Staff.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (staff != null && !string.IsNullOrWhiteSpace(staff.PhoneNumber))
            return staff.PhoneNumber;

        return null;
    }

    private static async Task<UserNotificationPreference> GetOrDefaultPrefsAsync(ApplicationDbContext db, Guid userId, CancellationToken ct)
    {
        var prefs = await db.UserNotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);
        return prefs ?? new UserNotificationPreference { UserId = userId };
    }

    private static bool IsMuted(UserNotificationPreference prefs, string category)
    {
        return category switch
        {
            "approvals" => prefs.MuteApprovals,
            "fees" => prefs.MuteFees,
            "attendance" => prefs.MuteAttendance,
            "system" => prefs.MuteSystem,
            _ => false
        };
    }

    private static async Task LogAsync(
        ApplicationDbContext db,
        Guid? notificationId,
        Guid? userId,
        string channel,
        string status,
        string? providerId,
        string? error,
        string? type,
        string? idempotencyKey)
    {
        db.NotificationDeliveryLogs.Add(new NotificationDeliveryLog
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            UserId = userId,
            Channel = channel,
            Status = status,
            ProviderMessageId = providerId ?? idempotencyKey,
            Error = error != null && error.Length > 1000 ? error[..1000] : error,
            Type = type,
            CreatedAt = DateTime.UtcNow
        });
        await Task.CompletedTask;
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= max ? s : s[..(max - 1)] + "…";
    }

    private sealed class DeliverySnapshot
    {
        public List<Guid> UserIds { get; set; } = new();
        public Dictionary<Guid, Guid> NotificationIds { get; set; } = new();
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string SmsBody { get; set; } = "";
        public string EmailSubject { get; set; } = "";
        public string EmailHtmlBody { get; set; } = "";
        public string Type { get; set; } = "";
        public string Category { get; set; } = "";
        public NotificationChannels Channels { get; set; }
        public Dictionary<string, string>? Data { get; set; }
        public List<string> ExtraEmails { get; set; } = new();
        public List<string> ExtraPhones { get; set; } = new();
        public string? IdempotencyKey { get; set; }
    }
}
