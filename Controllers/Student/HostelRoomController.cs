using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Controllers.Shared;
using sms.api.Data;
using sms.api.Interfaces.Student;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Student
{
    [Authorize(Roles = "Admin,Staff")]
    [Route("api/hostel-rooms")]
    public class HostelRoomController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostelRoomManager _manager;

        public HostelRoomController(ApplicationDbContext context, IHostelRoomManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? hostelId = null, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.HostelRooms.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (hostelId.HasValue) query = query.Where(x => x.HostelId == hostelId.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.RoomNumber != null && x.RoomNumber.ToLower().Contains(term)) ||
                    (x.RoomType != null && x.RoomType.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? hostelId = null, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var result = await _manager.GetPagedAsync(pageNumber, pageSize, hostelId, searchTerm, status, includeInactive);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.HostelRooms.FindAsync(id);
            return entity == null ? NotFound(new { message = "Hostel room not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HostelRoom entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (entity.HostelId == default) return BadRequest(new { message = "HostelId is required" });
            var hostelExists = await _context.Hostels.AnyAsync(h => h.Id == entity.HostelId && h.IsActive);
            if (!hostelExists) return BadRequest(new { message = "Hostel not found or inactive" });
            entity.Id = Guid.NewGuid();
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] HostelRoom entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.HostelRooms.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Hostel room not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.HostelRooms.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Hostel room not found" });
            existing.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
