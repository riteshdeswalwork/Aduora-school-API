using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/invitees")]
    public class InviteeController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IInviteeManager _manager;

        public InviteeController(ApplicationDbContext context, IInviteeManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? eventId = null, [FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.Invitees.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (eventId.HasValue) query = query.Where(x => x.EventId == eventId.Value);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(term)) ||
                    (x.Email != null && x.Email.ToLower().Contains(term)) ||
                    (x.ContactNumber != null && x.ContactNumber.ToLower().Contains(term)) ||
                    (x.Role != null && x.Role.ToLower().Contains(term)) ||
                    (x.RsvpStatus != null && x.RsvpStatus.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? eventId = null, [FromQuery] string? searchTerm = null, [FromQuery] string? rsvpStatus = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.Invitees.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (eventId.HasValue) query = query.Where(x => x.EventId == eventId.Value);
            if (!string.IsNullOrWhiteSpace(rsvpStatus))
                query = query.Where(x => x.RsvpStatus != null && x.RsvpStatus.ToLower() == rsvpStatus.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(term)) ||
                    (x.Email != null && x.Email.ToLower().Contains(term)) ||
                    (x.ContactNumber != null && x.ContactNumber.ToLower().Contains(term)) ||
                    (x.Role != null && x.Role.ToLower().Contains(term)) ||
                    (x.RsvpStatus != null && x.RsvpStatus.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.CreatedAt);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.Invitees.FindAsync(id);
            return entity == null ? NotFound(new { message = "Invitee not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Invitee entity)
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
        public async Task<IActionResult> Update(Guid id, [FromBody] Invitee entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.Invitees.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Invitee not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.Invitees.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Invitee not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
