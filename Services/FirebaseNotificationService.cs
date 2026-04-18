using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using sms.api.Data;
using Microsoft.EntityFrameworkCore;

namespace sms.api.Services
{
    public interface IFirebaseNotificationService
    {
        Task<bool> SendNotificationAsync(string token, string title, string body, Dictionary<string, string>? data = null);
        Task<bool> SendNotificationToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null);
        Task<bool> SendNotificationToMultipleUsersAsync(List<Guid> userIds, string title, string body, Dictionary<string, string>? data = null);
        Task<bool> SendNotificationToAllUsersAsync(string title, string body, Dictionary<string, string>? data = null);

        Task<NotificationSendResult> SendNotificationToUserWithStatusAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null);
        Task<NotificationSendResult> SendNotificationToMultipleUsersWithStatusAsync(List<Guid> userIds, string title, string body, Dictionary<string, string>? data = null);
        Task<NotificationSendResult> SendNotificationToAllUsersWithStatusAsync(string title, string body, Dictionary<string, string>? data = null);
    }

    public class NotificationSendResult
    {
        public int TargetUserCount { get; set; }
        public int UsersWithActiveTokens { get; set; }
        public int UsersWithoutActiveTokens { get; set; }

        public int TargetTokenCount { get; set; }
        public int SuccessTokenCount { get; set; }
        public int FailureTokenCount { get; set; }
    }

    public class FirebaseNotificationService : IFirebaseNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FirebaseNotificationService> _logger;

        public FirebaseNotificationService(ApplicationDbContext context, ILogger<FirebaseNotificationService> logger)
        {
            _context = context;
            _logger = logger;
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                try
                {
                    // Option 1: Use environment variable for service account JSON
                    var firebaseConfigPath = Environment.GetEnvironmentVariable("FIREBASE_CONFIG_PATH");
                    
                    if (!string.IsNullOrEmpty(firebaseConfigPath) && File.Exists(firebaseConfigPath))
                    {
                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(firebaseConfigPath)
                        });
                        _logger.LogInformation("Firebase initialized from config file");
                    }
                    else
                    {
                        // Option 2: Use Firebase config from appsettings.json
                        // You can also use FirebaseApp.Create() with credentials from appsettings
                        // For now, we'll use default credentials (requires GOOGLE_APPLICATION_CREDENTIALS env var)
                        FirebaseApp.Create();
                        _logger.LogInformation("Firebase initialized with default credentials");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Firebase. Notifications will not work.");
                }
            }
        }

        public async Task<bool> SendNotificationAsync(string token, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    _logger.LogWarning("Firebase not initialized. Cannot send notification.");
                    return false;
                }

                var message = new Message
                {
                    Token = token,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>(),
                    Webpush = new WebpushConfig
                    {
                        Notification = new WebpushNotification
                        {
                            Title = title,
                            Body = body,
                            Icon = "/aduora.svg",
                            Badge = "/aduora.svg"
                        }
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation($"Successfully sent notification. Message ID: {response}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to token: {token}");
                return false;
            }
        }

        public async Task<bool> SendNotificationToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null)
        {
            var result = await SendNotificationToUserWithStatusAsync(userId, title, body, data);
            return result.SuccessTokenCount > 0;
        }

        public async Task<bool> SendNotificationToMultipleUsersAsync(List<Guid> userIds, string title, string body, Dictionary<string, string>? data = null)
        {
            var result = await SendNotificationToMultipleUsersWithStatusAsync(userIds, title, body, data);
            return result.SuccessTokenCount > 0;
        }

        public async Task<bool> SendNotificationToAllUsersAsync(string title, string body, Dictionary<string, string>? data = null)
        {
            var result = await SendNotificationToAllUsersWithStatusAsync(title, body, data);
            return result.SuccessTokenCount > 0;
        }

        public async Task<NotificationSendResult> SendNotificationToUserWithStatusAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var tokens = await _context.FcmTokens
                    .Where(t => t.UserId == userId && t.IsActive)
                    .Select(t => t.Token)
                    .ToListAsync();

                var result = new NotificationSendResult
                {
                    TargetUserCount = 1,
                    UsersWithActiveTokens = tokens.Any() ? 1 : 0,
                    UsersWithoutActiveTokens = tokens.Any() ? 0 : 1,
                    TargetTokenCount = tokens.Count
                };

                if (!tokens.Any())
                {
                    _logger.LogWarning($"No active FCM tokens found for user: {userId}");
                    return result;
                }

                var successfulTokens = new List<string>();
                foreach (var token in tokens)
                {
                    if (await SendNotificationAsync(token, title, body, data))
                    {
                        successfulTokens.Add(token);
                    }
                }

                result.SuccessTokenCount = successfulTokens.Count;
                result.FailureTokenCount = result.TargetTokenCount - result.SuccessTokenCount;

                if (successfulTokens.Count > 0)
                {
                    var now = DateTime.UtcNow;
                    await _context.FcmTokens
                        .Where(t => t.UserId == userId && successfulTokens.Contains(t.Token))
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastUsedAt, now));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to user: {userId}");
                return new NotificationSendResult
                {
                    TargetUserCount = 1,
                    UsersWithActiveTokens = 0,
                    UsersWithoutActiveTokens = 1,
                    TargetTokenCount = 0,
                    SuccessTokenCount = 0,
                    FailureTokenCount = 0
                };
            }
        }

        public async Task<NotificationSendResult> SendNotificationToMultipleUsersWithStatusAsync(List<Guid> userIds, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var distinctUserIds = (userIds ?? new List<Guid>()).Where(id => id != Guid.Empty).Distinct().ToList();

                var tokenRows = await _context.FcmTokens
                    .Where(t => distinctUserIds.Contains(t.UserId) && t.IsActive)
                    .Select(t => new { t.UserId, t.Token })
                    .ToListAsync();

                var tokens = tokenRows.Select(t => t.Token).ToList();
                var usersWithTokens = tokenRows.Select(t => t.UserId).Distinct().Count();

                var result = new NotificationSendResult
                {
                    TargetUserCount = distinctUserIds.Count,
                    UsersWithActiveTokens = usersWithTokens,
                    UsersWithoutActiveTokens = Math.Max(0, distinctUserIds.Count - usersWithTokens),
                    TargetTokenCount = tokens.Count
                };

                if (!tokens.Any())
                {
                    _logger.LogWarning("No active FCM tokens found for provided users");
                    return result;
                }

                var successfulTokens = new List<string>();
                foreach (var token in tokens)
                {
                    if (await SendNotificationAsync(token, title, body, data))
                    {
                        successfulTokens.Add(token);
                    }
                }

                result.SuccessTokenCount = successfulTokens.Count;
                result.FailureTokenCount = result.TargetTokenCount - result.SuccessTokenCount;

                if (successfulTokens.Count > 0)
                {
                    var now = DateTime.UtcNow;
                    await _context.FcmTokens
                        .Where(t => successfulTokens.Contains(t.Token))
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastUsedAt, now));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to multiple users");
                return new NotificationSendResult
                {
                    TargetUserCount = userIds?.Count ?? 0,
                    UsersWithActiveTokens = 0,
                    UsersWithoutActiveTokens = userIds?.Count ?? 0,
                    TargetTokenCount = 0,
                    SuccessTokenCount = 0,
                    FailureTokenCount = 0
                };
            }
        }

        public async Task<NotificationSendResult> SendNotificationToAllUsersWithStatusAsync(string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var tokens = await _context.FcmTokens
                    .Where(t => t.IsActive)
                    .Select(t => t.Token)
                    .ToListAsync();

                var usersWithTokens = await _context.FcmTokens
                    .Where(t => t.IsActive)
                    .Select(t => t.UserId)
                    .Distinct()
                    .CountAsync();

                var totalUsers = await _context.Users.CountAsync();

                var result = new NotificationSendResult
                {
                    TargetUserCount = totalUsers,
                    UsersWithActiveTokens = usersWithTokens,
                    UsersWithoutActiveTokens = Math.Max(0, totalUsers - usersWithTokens),
                    TargetTokenCount = tokens.Count
                };

                if (!tokens.Any())
                {
                    _logger.LogWarning("No active FCM tokens found");
                    return result;
                }

                var successfulTokens = new List<string>();
                foreach (var token in tokens)
                {
                    if (await SendNotificationAsync(token, title, body, data))
                    {
                        successfulTokens.Add(token);
                    }
                }

                result.SuccessTokenCount = successfulTokens.Count;
                result.FailureTokenCount = result.TargetTokenCount - result.SuccessTokenCount;

                if (successfulTokens.Count > 0)
                {
                    var now = DateTime.UtcNow;
                    await _context.FcmTokens
                        .Where(t => successfulTokens.Contains(t.Token))
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastUsedAt, now));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to all users");
                return new NotificationSendResult
                {
                    TargetUserCount = 0,
                    UsersWithActiveTokens = 0,
                    UsersWithoutActiveTokens = 0,
                    TargetTokenCount = 0,
                    SuccessTokenCount = 0,
                    FailureTokenCount = 0
                };
            }
        }
    }
}
