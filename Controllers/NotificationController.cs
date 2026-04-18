using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Models;
using sms.api.Services;
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
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            IFirebaseNotificationService firebaseService,
            ApplicationDbContext context,
            ILogger<NotificationController> logger)
        {
            _firebaseService = firebaseService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Register FCM token for the current user
        /// </summary>
        [HttpPost("register-token")]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized();
                }

                if (string.IsNullOrEmpty(request.Token))
                {
                    return BadRequest(new { message = "Token is required" });
                }

                // Check if token already exists for this user
                var existingToken = await _context.FcmTokens
                    .FirstOrDefaultAsync(t => t.Token == request.Token && t.UserId == userId);

                if (existingToken != null)
                {
                    // Update existing token
                    existingToken.IsActive = true;
                    existingToken.LastUsedAt = DateTime.UtcNow;
                    existingToken.DeviceType = request.DeviceType;
                    existingToken.DeviceInfo = request.DeviceInfo;
                }
                else
                {
                    // Create new token
                    var fcmToken = new FcmToken
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Token = request.Token,
                        DeviceType = request.DeviceType ?? "Web",
                        DeviceInfo = request.DeviceInfo,
                        CreatedAt = DateTime.UtcNow,
                        LastUsedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.FcmTokens.Add(fcmToken);
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

        /// <summary>
        /// Send notification to specific user
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            try
            {
                var result = await _firebaseService.SendNotificationToUserWithStatusAsync(
                    request.UserId,
                    request.Title,
                    request.Body,
                    request.Data
                );

                if (result.SuccessTokenCount > 0)
                {
                    return Ok(new
                    {
                        message = "Notification sent successfully",
                        result.TargetUserCount,
                        result.UsersWithActiveTokens,
                        result.UsersWithoutActiveTokens,
                        result.TargetTokenCount,
                        result.SuccessTokenCount,
                        result.FailureTokenCount
                    });
                }

                return BadRequest(new
                {
                    message = "Failed to send notification",
                    result.TargetUserCount,
                    result.UsersWithActiveTokens,
                    result.UsersWithoutActiveTokens,
                    result.TargetTokenCount,
                    result.SuccessTokenCount,
                    result.FailureTokenCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                return StatusCode(500, new { message = "Error sending notification" });
            }
        }

        /// <summary>
        /// Send notification to multiple users
        /// </summary>
        [HttpPost("send-multiple")]
        public async Task<IActionResult> SendNotificationToMultiple([FromBody] SendNotificationMultipleRequest request)
        {
            try
            {
                var result = await _firebaseService.SendNotificationToMultipleUsersWithStatusAsync(
                    request.UserIds,
                    request.Title,
                    request.Body,
                    request.Data
                );

                if (result.SuccessTokenCount > 0)
                {
                    return Ok(new
                    {
                        message = "Notifications sent successfully",
                        result.TargetUserCount,
                        result.UsersWithActiveTokens,
                        result.UsersWithoutActiveTokens,
                        result.TargetTokenCount,
                        result.SuccessTokenCount,
                        result.FailureTokenCount
                    });
                }

                return BadRequest(new
                {
                    message = "Failed to send notifications",
                    result.TargetUserCount,
                    result.UsersWithActiveTokens,
                    result.UsersWithoutActiveTokens,
                    result.TargetTokenCount,
                    result.SuccessTokenCount,
                    result.FailureTokenCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications");
                return StatusCode(500, new { message = "Error sending notifications" });
            }
        }

        /// <summary>
        /// Send notification to all users
        /// </summary>
        [HttpPost("send-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotificationToAll([FromBody] SendNotificationAllRequest request)
        {
            try
            {
                var result = await _firebaseService.SendNotificationToAllUsersWithStatusAsync(
                    request.Title,
                    request.Body,
                    request.Data
                );

                if (result.SuccessTokenCount > 0)
                {
                    return Ok(new
                    {
                        message = "Notifications sent successfully",
                        result.TargetUserCount,
                        result.UsersWithActiveTokens,
                        result.UsersWithoutActiveTokens,
                        result.TargetTokenCount,
                        result.SuccessTokenCount,
                        result.FailureTokenCount
                    });
                }

                return BadRequest(new
                {
                    message = "Failed to send notifications",
                    result.TargetUserCount,
                    result.UsersWithActiveTokens,
                    result.UsersWithoutActiveTokens,
                    result.TargetTokenCount,
                    result.SuccessTokenCount,
                    result.FailureTokenCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications to all users");
                return StatusCode(500, new { message = "Error sending notifications" });
            }
        }

        /// <summary>
        /// Get user's FCM tokens
        /// </summary>
        [HttpGet("tokens")]
        public async Task<IActionResult> GetUserTokens()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized();
                }

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

        /// <summary>
        /// Remove FCM token
        /// </summary>
        [HttpDelete("token/{tokenId}")]
        public async Task<IActionResult> RemoveToken(Guid tokenId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized();
                }

                var token = await _context.FcmTokens
                    .FirstOrDefaultAsync(t => t.Id == tokenId && t.UserId == userId);

                if (token == null)
                {
                    return NotFound(new { message = "Token not found" });
                }

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

    // DTOs
    public class RegisterTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public string? DeviceType { get; set; }
        public string? DeviceInfo { get; set; }
    }

    public class SendNotificationRequest
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Dictionary<string, string>? Data { get; set; }
    }

    public class SendNotificationMultipleRequest
    {
        public List<Guid> UserIds { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Dictionary<string, string>? Data { get; set; }
    }

    public class SendNotificationAllRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Dictionary<string, string>? Data { get; set; }
    }
}
