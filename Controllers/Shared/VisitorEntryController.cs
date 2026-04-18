using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/visitor-entries")]
    public class VisitorEntryController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IVisitorEntryManager _manager;

        public VisitorEntryController(ApplicationDbContext context, IVisitorEntryManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false, [FromQuery] bool activeOnly = false, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var query = _context.VisitorEntries.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (activeOnly) query = query.Where(x => x.Status == "In" && x.TimeOut == null);
            if (fromDate.HasValue) query = query.Where(x => x.TimeIn >= fromDate.Value);
            if (toDate.HasValue) { var end = toDate.Value.Date.AddDays(1); query = query.Where(x => x.TimeIn < end); }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.VisitorName != null && x.VisitorName.ToLower().Contains(term)) ||
                    (x.ContactNumber != null && x.ContactNumber.ToLower().Contains(term)) ||
                    (x.Purpose != null && x.Purpose.ToLower().Contains(term)) ||
                    (x.PersonToMeet != null && x.PersonToMeet.ToLower().Contains(term)) ||
                    (x.VehicleNumber != null && x.VehicleNumber.ToLower().Contains(term)) ||
                    (x.Address != null && x.Address.ToLower().Contains(term)) ||
                    (x.IdNumber != null && x.IdNumber.ToLower().Contains(term)) ||
                    (x.BadgeNumber != null && x.BadgeNumber.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.TimeIn).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false, [FromQuery] bool activeOnly = false, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var query = _context.VisitorEntries.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (activeOnly) query = query.Where(x => x.Status == "In" && x.TimeOut == null);
            if (fromDate.HasValue) query = query.Where(x => x.TimeIn >= fromDate.Value);
            if (toDate.HasValue) { var end = toDate.Value.Date.AddDays(1); query = query.Where(x => x.TimeIn < end); }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.VisitorName != null && x.VisitorName.ToLower().Contains(term)) ||
                    (x.ContactNumber != null && x.ContactNumber.ToLower().Contains(term)) ||
                    (x.Purpose != null && x.Purpose.ToLower().Contains(term)) ||
                    (x.PersonToMeet != null && x.PersonToMeet.ToLower().Contains(term)) ||
                    (x.VehicleNumber != null && x.VehicleNumber.ToLower().Contains(term)) ||
                    (x.Address != null && x.Address.ToLower().Contains(term)) ||
                    (x.IdNumber != null && x.IdNumber.ToLower().Contains(term)) ||
                    (x.BadgeNumber != null && x.BadgeNumber.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.TimeIn);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.VisitorEntries.FindAsync(id);
            return entity == null ? NotFound(new { message = "Visitor entry not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VisitorEntry entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            entity.Id = Guid.NewGuid();
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = null;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] VisitorEntry entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.VisitorEntries.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Visitor entry not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpPut("{id}/checkout")]
        public async Task<IActionResult> CheckOut(Guid id, [FromBody] DateTime? timeOut = null)
        {
            var existing = await _context.VisitorEntries.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Visitor entry not found" });
            if (!existing.IsActive) return BadRequest(new { message = "Visitor entry is inactive" });
            if (existing.Status == "Out" && existing.TimeOut.HasValue) return BadRequest(new { message = "Visitor already checked out" });
            existing.TimeOut = timeOut ?? DateTime.UtcNow;
            existing.Status = "Out";
            existing.CheckOutById = GetCurrentUserId();
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.VisitorEntries.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Visitor entry not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
