using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Services;

namespace sms.api.Controllers.Auth
{
    public class AuthController : ApiControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly IRecaptchaService _recaptchaService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserManager userManager, IRecaptchaService recaptchaService, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _recaptchaService = recaptchaService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation($"Login attempt for user: {request.Username}");

            // Verify reCAPTCHA token when provided (optional when empty for development / portal)
            if (!string.IsNullOrWhiteSpace(request.RecaptchaToken) && !await _recaptchaService.VerifyTokenAsync(request.RecaptchaToken, "login"))
            {
                _logger.LogWarning("reCAPTCHA verification failed");
                return BadRequest(new { message = "reCAPTCHA verification failed. Please try again." });
            }

            // Get user information
            var user = await _userManager.GetByUsernameAsync(request.Username);
            if (user == null)
                return Unauthorized(new { message = "Username or password is incorrect" });

            // Verify password and generate token
            var token = await _userManager.AuthenticateAsync(request.Username, request.Password);
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Username or password is incorrect" });

            // Role-based access: permission codes for frontend (nav + page access + buttons)
            var permissions = await _userManager.GetUserPermissionCodesAsync(user.Id);

            // Profile photo as data URL so frontend can show without another API call
            string profilePhoto = null;
            if (user.ProfilePhotoData != null && user.ProfilePhotoData.Length > 0)
            {
                var contentType = !string.IsNullOrEmpty(user.ProfilePhotoContentType) ? user.ProfilePhotoContentType : "image/jpeg";
                profilePhoto = "data:" + contentType + ";base64," + Convert.ToBase64String(user.ProfilePhotoData);
            }

            // Return complete user profile with token (name, email, photo – no separate profile API needed)
            return Ok(new {
                token,
                username = user.Username,
                role = user.Role,
                fullName = user.FullName?.Trim() ?? user.Username,
                userId = user.Id.ToString(),
                permissions = permissions,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                phone = user.PhoneNumber,
                profilePhoto
            });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid token" });

            var user = await _userManager.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var permissions = await _userManager.GetUserPermissionCodesAsync(userId);
            return Ok(new
            {
                userId = user.Id.ToString(),
                username = user.Username,
                fullName = user.FullName,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                role = user.Role,
                permissions = permissions
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _logger.LogInformation("User logged out");
            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpGet("permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
            var permissions = await _userManager.GetUserPermissionCodesAsync(userId);
            return Ok(permissions);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst("nameid")?.Value;
            if (userIdClaim == null)
                return BadRequest(new { message = "User ID not found in token" });

            var userId = Guid.Parse(userIdClaim);
            var result = await _userManager.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
            if (!result)
                return BadRequest(new { message = "Old password is incorrect" });

            return Ok(new { message = "Password changed successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("reset-password/{id}")]
        public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request)
        {
            var result = await _userManager.ResetPasswordAsync(id, request.NewPassword);
            if (!result)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "Password reset successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("hash-plain-passwords")]
        public async Task<IActionResult> HashPlainTextPasswords()
        {
            try
            {
                var updatedCount = await _userManager.HashPlainTextPasswordsAsync();
                return Ok(new {
                    message = $"Successfully hashed {updatedCount} plain text passwords",
                    updatedCount = updatedCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error hashing passwords: {ex.Message}" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("cleanup-duplicate-users")]
        public async Task<IActionResult> CleanupDuplicateUsers()
        {
            try
            {
                var cleanedCount = await _userManager.CleanupDuplicateUsersAsync();
                return Ok(new {
                    message = $"Successfully cleaned up {cleanedCount} duplicate users",
                    cleanedCount = cleanedCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error cleaning up duplicates: {ex.Message}" });
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RecaptchaToken { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
