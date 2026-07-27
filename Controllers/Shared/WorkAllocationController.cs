using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/work-allocations")]
    public class WorkAllocationController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkAllocationManager _manager;

        public WorkAllocationController(ApplicationDbContext context, IWorkAllocationManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.WorkAllocations.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(term)) ||
                    (x.Description != null && x.Description.ToLower().Contains(term)) ||
                    (x.Priority != null && x.Priority.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] Guid? assignedToStaffId = null, [FromQuery] Guid? assignedToUserId = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.WorkAllocations.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status != null && x.Status.Trim().ToLower() == status.Trim().ToLower());
            if (assignedToStaffId.HasValue) query = query.Where(x => x.AssignedToStaffId == assignedToStaffId.Value);
            if (assignedToUserId.HasValue)
            {
                var staffId = await _context.Staff.AsNoTracking().Where(s => s.UserId == assignedToUserId.Value).Select(s => s.Id).FirstOrDefaultAsync();
                if (staffId != default) query = query.Where(x => x.AssignedToStaffId == staffId);
                else query = query.Where(x => false);
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(term)) ||
                    (x.Description != null && x.Description.ToLower().Contains(term)) ||
                    (x.Priority != null && x.Priority.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)) ||
                    (x.Complexity != null && x.Complexity.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.CreatedAt);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.WorkAllocations.FindAsync(id);
            return entity == null ? NotFound(new { message = "Work allocation not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkAllocation entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            entity.Id = Guid.NewGuid();
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = null;
            if (!entity.AssignedByUserId.HasValue)
                entity.AssignedByUserId = GetCurrentUserId();
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] WorkAllocation entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.WorkAllocations.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Work allocation not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            if (!entity.AssignedByUserId.HasValue)
                entity.AssignedByUserId = existing.AssignedByUserId ?? GetCurrentUserId();
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            var existing = await _manager.CompleteAsync(id);
            if (existing == null) return NotFound(new { message = "Work allocation not found" });
            if (!existing.IsActive) return BadRequest(new { message = "Work allocation is inactive" });
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.WorkAllocations.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Work allocation not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
