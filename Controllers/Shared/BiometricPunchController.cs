using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using System.Security.Claims;

namespace sms.api.Controllers.Shared
{
    /// <summary>
    /// Fingerprint device punch ingest: match device user → mark that day's attendance.
    /// Auth: Admin/Staff JWT, or header X-Device-Api-Key matching the device ApiKey.
    /// </summary>
    [Route("api/biometric")]
    [ApiController]
    public class BiometricPunchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBiometricAttendanceService _attendanceService;

        public BiometricPunchController(ApplicationDbContext context, IBiometricAttendanceService attendanceService)
        {
            _context = context;
            _attendanceService = attendanceService;
        }

        public class PunchBody
        {
            public Guid? DeviceId { get; set; }
            public string? DeviceIdentifier { get; set; }
            /// <summary>User id enrolled on the fingerprint machine.</summary>
            public string? ExternalUserId { get; set; }
            public string LogType { get; set; } = "IN";
            public DateTime? LoggedAt { get; set; }
            public Guid? StaffId { get; set; }
            public Guid? StudentId { get; set; }
            public string? RawData { get; set; }
        }

        /// <summary>
        /// Device or admin posts a fingerprint match punch.
        /// POST api/biometric/punch
        /// Header (optional): X-Device-Api-Key: {device.apiKey}
        /// </summary>
        [HttpPost("punch")]
        [AllowAnonymous]
        public async Task<IActionResult> Punch([FromBody] PunchBody body)
        {
            if (body == null)
                return BadRequest(new { message = "Body required" });

            var apiKey = Request.Headers["X-Device-Api-Key"].FirstOrDefault()
                ?? Request.Headers["X-Api-Key"].FirstOrDefault();

            var isJwt = User?.Identity?.IsAuthenticated == true;
            if (!isJwt)
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                    return Unauthorized(new { message = "Provide Bearer token or X-Device-Api-Key header" });

                var device = await ResolveDeviceForApiKeyAsync(body.DeviceId, body.DeviceIdentifier, apiKey.Trim());
                if (device == null)
                    return Unauthorized(new { message = "Invalid device API key or device not found" });

                body.DeviceId = device.Id;
                body.DeviceIdentifier = device.DeviceIdentifier;
            }
            else if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Forbid();
            }

            Guid? actingUserId = null;
            if (isJwt)
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id");
                if (claim != null && Guid.TryParse(claim.Value, out var uid))
                    actingUserId = uid;
            }

            var result = await _attendanceService.ProcessPunchAsync(new BiometricPunchRequest
            {
                DeviceId = body.DeviceId,
                DeviceIdentifier = body.DeviceIdentifier,
                ExternalUserId = body.ExternalUserId ?? string.Empty,
                LogType = body.LogType ?? "IN",
                LoggedAt = body.LoggedAt,
                StaffId = body.StaffId,
                StudentId = body.StudentId,
                RawData = body.RawData
            }, actingUserId);

            if (!result.Success)
                return BadRequest(new { message = result.Message, result });

            return Ok(result);
        }

        private async Task<Models.BiometricDevice?> ResolveDeviceForApiKeyAsync(Guid? deviceId, string? deviceIdentifier, string apiKey)
        {
            var q = _context.BiometricDevices.AsNoTracking()
                .Where(d => d.IsActive && d.ApiKey != null && d.ApiKey == apiKey);

            if (deviceId.HasValue && deviceId != Guid.Empty)
                q = q.Where(d => d.Id == deviceId.Value);
            else if (!string.IsNullOrWhiteSpace(deviceIdentifier))
                q = q.Where(d => d.DeviceIdentifier == deviceIdentifier.Trim());

            return await q.FirstOrDefaultAsync();
        }
    }
}
