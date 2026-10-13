using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/enquiries")]
    public class EnquiryController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEnquiryManager _manager;

        public EnquiryController(ApplicationDbContext context, IEnquiryManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.Enquiries.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(term)) ||
                    (x.ParentName != null && x.ParentName.ToLower().Contains(term)) ||
                    (x.ContactNumber != null && x.ContactNumber.ToLower().Contains(term)) ||
                    (x.Email != null && x.Email.ToLower().Contains(term)) ||
                    (x.CurrentSchool != null && x.CurrentSchool.ToLower().Contains(term)) ||
                    (x.SeekingClass != null && x.SeekingClass.ToLower().Contains(term)) ||
                    (x.CourseInterested != null && x.CourseInterested.ToLower().Contains(term)) ||
                    (x.Source != null && x.Source.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.EnquiryDate).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] string? priority = null, [FromQuery] string? source = null, [FromQuery] bool? hasFollowUp = null, [FromQuery] bool includeInactive = false)
        {
            var result = await _manager.GetPagedAsync(pageNumber, pageSize, searchTerm, status, priority, source, hasFollowUp, includeInactive);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.Enquiries.FindAsync(id);
            return entity == null ? NotFound(new { message = "Enquiry not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Enquiry entity)
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
        public async Task<IActionResult> Update(Guid id, [FromBody] Enquiry entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.Enquiries.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Enquiry not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.Enquiries.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Enquiry not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
