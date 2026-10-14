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
    [Route("api/hostel-allocations")]
    public class HostelAllocationController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostelAllocationManager _manager;

        public HostelAllocationController(ApplicationDbContext context, IHostelAllocationManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? academicYearId = null, [FromQuery] Guid? hostelRoomId = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.HostelAllocations.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (hostelRoomId.HasValue) query = query.Where(x => x.HostelRoomId == hostelRoomId.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            var items = await query.OrderByDescending(x => x.FromDate).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? academicYearId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? hostelRoomId = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var result = await _manager.GetPagedAsync(yearId.Value, pageNumber, pageSize, hostelRoomId, status, includeInactive);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.HostelAllocations.FindAsync(id);
            return entity == null ? NotFound(new { message = "Hostel allocation not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HostelAllocation entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (entity.HostelRoomId == default) return BadRequest(new { message = "HostelRoomId is required" });
            var roomExists = await _context.HostelRooms.AnyAsync(r => r.Id == entity.HostelRoomId && r.IsActive);
            if (!roomExists) return BadRequest(new { message = "Hostel room not found or inactive" });
            entity.Id = Guid.NewGuid();
            if (entity.AcademicYearId == default)
            {
                var yearId = GetCurrentAcademicYearId();
                if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });
                entity.AcademicYearId = yearId.Value;
            }
            entity.IsActive = true;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] HostelAllocation entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.HostelAllocations.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Hostel allocation not found" });
            entity.Id = id;
            if (entity.AcademicYearId == default) entity.AcademicYearId = existing.AcademicYearId;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.HostelAllocations.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Hostel allocation not found" });
            existing.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
