using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/biometric-devices")]
    public class BiometricDeviceController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBiometricDeviceManager _manager;

        public BiometricDeviceController(ApplicationDbContext context, IBiometricDeviceManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.BiometricDevices.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.DeviceIdentifier != null && x.DeviceIdentifier.ToLower().Contains(term)) ||
                    (x.Location != null && x.Location.ToLower().Contains(term)) ||
                    (x.Model != null && x.Model.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.BiometricDevices.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status != null && x.Status.Trim().ToLower() == status.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.DeviceIdentifier != null && x.DeviceIdentifier.ToLower().Contains(term)) ||
                    (x.Location != null && x.Location.ToLower().Contains(term)) ||
                    (x.Model != null && x.Model.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.CreatedAt);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.BiometricDevices.FindAsync(id);
            return entity == null ? NotFound(new { message = "Biometric device not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BiometricDevice entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            entity.Id = Guid.NewGuid();
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BiometricDevice entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.BiometricDevices.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Biometric device not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.BiometricDevices.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Biometric device not found" });
            existing.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
