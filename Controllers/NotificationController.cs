using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Models;
using sms.api.Services;
using sms.api.Services.Data;
using System.Security.Claims;

namespace sms.api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Route("api/notification")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IFirebaseNotificationService _firebaseService;
        private readonly INotificationOrchestrator _orchestrator;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationController> _logger;
        private readonly ISqlDataAccess _sqlDataAccess;

        public NotificationController(
            IFirebaseNotificationService firebaseService,
            INotificationOrchestrator orchestrator,
            ApplicationDbContext context,
            ILogger<NotificationController> logger,
            ISqlDataAccess sqlDataAccess)
        {
            _firebaseService = firebaseService;
            _orchestrator = orchestrator;
            _context = context;
            _logger = logger;
            _sqlDataAccess = sqlDataAccess;
        }

        private bool TryGetUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("id")?.Value;
            return !string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out userId);
        }

        private static NotificationChannels ParseChannels(SendChannelsDto? channels)
        {
            if (channels == null)
                return NotificationChannels.All;

            var c = NotificationChannels.None;
            if (channels.InApp) c |= NotificationChannels.InApp;
            if (channels.Push) c |= NotificationChannels.Push;
            if (channels.Email) c |= NotificationChannels.Email;
            if (channels.Sms) c |= NotificationChannels.Sms;
            return c == NotificationChannels.None ? NotificationChannels.Push | NotificationChannels.InApp : c;
        }

        [HttpPost("register-token")]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenRequest request)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                if (string.IsNullOrEmpty(request.Token))
                    return BadRequest(new { message = "Token is required" });

                var existingToken = await _context.FcmTokens
                    .FirstOrDefaultAsync(t => t.Token == request.Token && t.UserId == userId);

                if (existingToken != null)
                {
                    existingToken.IsActive = true;
                    existingToken.LastUsedAt = DateTime.UtcNow;
                    existingToken.DeviceType = request.DeviceType;
                    existingToken.DeviceInfo = request.DeviceInfo;
                }
                else
                {
                    _context.FcmTokens.Add(new FcmToken
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Token = request.Token,
                        DeviceType = request.DeviceType ?? "Web",
                        DeviceInfo = request.DeviceInfo,
                        CreatedAt = DateTime.UtcNow,
                        LastUsedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Token registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering FCM token");
                return StatusCode(500, new { message = "Error registering token" });
            }
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox([FromQuery] bool unreadOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@UnreadOnly", unreadOnly);
            parameters.Add("@Page", page);
            parameters.Add("@PageSize", pageSize);

            using var connection = await _sqlDataAccess.CreateOpenConnectionAsync();
            using var multi = await connection.QueryMultipleAsync(
                "dbo.sp_InAppNotifications_GetInboxPaged", parameters, commandType: System.Data.CommandType.StoredProcedure);

            var total = await multi.ReadSingleAsync<int>();
            var unread = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<InboxNotificationDto>()).ToList();

            return Ok(new { items, total, unread, page, pageSize });
        }

        [HttpPost("inbox/{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var n = await _context.InAppNotifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (n == null) return NotFound();

            n.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Marked as read" });
        }

        [HttpPost("inbox/read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            await _context.InAppNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

            return Ok(new { message = "All marked as read" });
        }

        [HttpGet("preferences")]
        public async Task<IActionResult> GetPreferences()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var prefs = await _context.UserNotificationPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            prefs ??= new UserNotificationPreference { UserId = userId };
            return Ok(prefs);
        }

        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UserNotificationPreference body)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var prefs = await _context.UserNotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (prefs == null)
            {
                prefs = new UserNotificationPreference
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                _context.UserNotificationPreferences.Add(prefs);
            }

            prefs.EnablePush = body.EnablePush;
            prefs.EnableEmail = body.EnableEmail;
            prefs.EnableSms = body.EnableSms;
            prefs.EnableInApp = body.EnableInApp;
            prefs.MuteApprovals = body.MuteApprovals;
            prefs.MuteFees = body.MuteFees;
            prefs.MuteAttendance = body.MuteAttendance;
            prefs.MuteSystem = body.MuteSystem;
            prefs.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(prefs);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            try
            {
                var dispatch = await _orchestrator.NotifyAsync(new NotificationRequest
                {
                    UserIds = new List<Guid> { request.UserId },
                    Title = request.Title,
                    Body = request.Body,
                    Type = request.Type ?? "manual",
                    Category = "general",
                    Channels = ParseChannels(request.Channels),
                    Data = request.Data,
                    SmsBody = request.SmsBody,
                    EmailSubject = request.EmailSubject,
                    EmailHtmlBody = request.EmailHtmlBody
                });

                return Ok(new { message = "Notification queued", dispatch });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                return StatusCode(500, new { message = "Error sending notification" });
            }
        }

        [HttpPost("send-multiple")]
        public async Task<IActionResult> SendNotificationToMultiple([FromBody] SendNotificationMultipleRequest request)
        {
            try
            {
                var dispatch = await _orchestrator.NotifyAsync(new NotificationRequest
                {
                    UserIds = request.UserIds ?? new List<Guid>(),
                    Title = request.Title,
                    Body = request.Body,
                    Type = request.Type ?? "manual",
                    Category = "general",
                    Channels = ParseChannels(request.Channels),
                    Data = request.Data,
                    SmsBody = request.SmsBody,
                    EmailSubject = request.EmailSubject,
                    EmailHtmlBody = request.EmailHtmlBody
                });

                return Ok(new { message = "Notifications queued", dispatch });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications");
                return StatusCode(500, new { message = "Error sending notifications" });
            }
        }

        [HttpPost("send-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotificationToAll([FromBody] SendNotificationAllRequest request)
        {
            try
            {
                var userIds = await _context.Users.AsNoTracking()
                    .Where(u => u.IsActive)
                    .Select(u => u.Id)
                    .ToListAsync();

                var dispatch = await _orchestrator.NotifyAsync(new NotificationRequest
                {
                    UserIds = userIds,
                    Title = request.Title,
                    Body = request.Body,
                    Type = request.Type ?? "manual.broadcast",
                    Category = "general",
                    Channels = ParseChannels(request.Channels),
                    Data = request.Data,
                    SmsBody = request.SmsBody,
                    EmailSubject = request.EmailSubject,
                    EmailHtmlBody = request.EmailHtmlBody
                });

                return Ok(new { message = "Broadcast queued", dispatch });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications to all users");
                return StatusCode(500, new { message = "Error sending notifications" });
            }
        }

        [HttpGet("tokens")]
        public async Task<IActionResult> GetUserTokens()
        {
            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                var tokens = await _context.FcmTokens
                    .Where(t => t.UserId == userId && t.IsActive)
                    .Select(t => new
                    {
                        t.Id,
                        t.DeviceType,
                        t.DeviceInfo,
                        t.CreatedAt,
                        t.LastUsedAt
                    })
                    .ToListAsync();

                return Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user tokens");
                return StatusCode(500, new { message = "Error getting tokens" });
            }
        }

        [HttpDelete("token/{tokenId}")]
        public async Task<IActionResult> RemoveToken(Guid tokenId)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                var token = await _context.FcmTokens
                    .FirstOrDefaultAsync(t => t.Id == tokenId && t.UserId == userId);

                if (token == null)
                    return NotFound(new { message = "Token not found" });

                token.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Token removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing token");
                return StatusCode(500, new { message = "Error removing token" });
            }
        }
    }

    public class RegisterTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public string? DeviceType { get; set; }
        public string? DeviceInfo { get; set; }
    }

    public class SendChannelsDto
    {
        public bool InApp { get; set; } = true;
        public bool Push { get; set; } = true;
        public bool Email { get; set; } = true;
        public bool Sms { get; set; } = false;
    }

    public class SendNotificationRequest
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? SmsBody { get; set; }
        public string? EmailSubject { get; set; }
        public string? EmailHtmlBody { get; set; }
        public SendChannelsDto? Channels { get; set; }
        public Dictionary<string, string>? Data { get; set; }
    }

    public class SendNotificationMultipleRequest
    {
        public List<Guid> UserIds { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? SmsBody { get; set; }
        public string? EmailSubject { get; set; }
        public string? EmailHtmlBody { get; set; }
        public SendChannelsDto? Channels { get; set; }
        public Dictionary<string, string>? Data { get; set; }
    }

    public class SendNotificationAllRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? SmsBody { get; set; }
        public string? EmailSubject { get; set; }
        public string? EmailHtmlBody { get; set; }
        public SendChannelsDto? Channels { get; set; }
        public Dictionary<string, string>? Data { get; set; }
    }

    /// <summary>Flat row shape returned by dbo.sp_InAppNotifications_GetInboxPaged -
    /// mirrors the inbox's original anonymous-object projection field-for-field.</summary>
    public class InboxNotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public string Severity { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? DataJson { get; set; }
    }
}
