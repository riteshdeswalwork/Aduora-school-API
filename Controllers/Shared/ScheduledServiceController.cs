using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/scheduled-services")]
    public class ScheduledServiceController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IScheduledServiceManager _manager;

        public ScheduledServiceController(ApplicationDbContext context, IScheduledServiceManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.ScheduledServices.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(term)) ||
                    (x.Category != null && x.Category.ToLower().Contains(term)) ||
                    (x.Frequency != null && x.Frequency.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.ScheduledFor).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] string? category = null, [FromQuery] bool includeInactive = false)
        {
            var result = await _manager.GetPagedAsync(pageNumber, pageSize, searchTerm, status, category, includeInactive);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.ScheduledServices.FindAsync(id);
            return entity == null ? NotFound(new { message = "Scheduled service not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ScheduledService entity)
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
        public async Task<IActionResult> Update(Guid id, [FromBody] ScheduledService entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.ScheduledServices.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Scheduled service not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.ScheduledServices.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Scheduled service not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
