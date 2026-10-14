using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/biometric-logs")]
    public class BiometricLogController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBiometricLogManager _manager;

        public BiometricLogController(ApplicationDbContext context, IBiometricLogManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? deviceId = null, [FromQuery] Guid? userId = null, [FromQuery] Guid? studentId = null, [FromQuery] Guid? staffId = null, [FromQuery] string? logType = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.BiometricLogs.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (deviceId.HasValue) query = query.Where(x => x.DeviceId == deviceId.Value);
            if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);
            if (studentId.HasValue) query = query.Where(x => x.StudentId == studentId.Value);
            if (staffId.HasValue) query = query.Where(x => x.StaffId == staffId.Value);
            if (!string.IsNullOrWhiteSpace(logType)) query = query.Where(x => x.LogType == logType);
            if (from.HasValue) query = query.Where(x => x.LoggedAt >= from.Value);
            if (to.HasValue) query = query.Where(x => x.LoggedAt <= to.Value);
            var items = await query.OrderByDescending(x => x.LoggedAt).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? deviceId = null, [FromQuery] Guid? userId = null, [FromQuery] Guid? studentId = null, [FromQuery] Guid? staffId = null, [FromQuery] string? logType = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] bool includeInactive = false)
        {
            var result = await _manager.GetPagedAsync(pageNumber, pageSize, deviceId, userId, studentId, staffId, logType, from, to, includeInactive);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.BiometricLogs.FindAsync(id);
            return entity == null ? NotFound(new { message = "Biometric log not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BiometricLog entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            entity.Id = Guid.NewGuid();
            entity.IsActive = true;
            if (entity.LoggedAt == default) entity.LoggedAt = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(entity.LogType)) entity.LogType = "IN";
            if (!entity.UserId.HasValue) entity.UserId = GetCurrentUserId();
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BiometricLog entity)
        {
            return BadRequest(new { message = "Updating biometric logs is not supported." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return BadRequest(new { message = "Deleting biometric logs is not supported." });
        }
    }
}
