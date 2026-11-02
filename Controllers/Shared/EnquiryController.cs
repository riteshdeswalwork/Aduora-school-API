using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    // NOTE: Intentionally extends ApiControllerBase (not ExtraModulesControllerBase) so each
    // action below can declare its own [Authorize(Roles=...)]. AuthorizeAttribute has
    // AllowMultiple=true + Inherited=true, so an attribute added here would otherwise STACK
    // (AND) with ExtraModulesControllerBase's class-level [Authorize(Roles="Admin,Staff")] and
    // could never broaden access for Receptionist/Principal. ApiControllerBase exposes the same
    // helper methods and has no Authorize attribute of its own, so there's no such conflict.
    [ApiController]
    [Route("api/enquiries")]
    public class EnquiryController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEnquiryManager _manager;

        public EnquiryController(ApplicationDbContext context, IEnquiryManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [Authorize(Roles = "Admin,Staff,Receptionist,Principal")]
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

        [Authorize(Roles = "Admin,Staff,Receptionist,Principal")]
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] string? priority = null, [FromQuery] string? source = null, [FromQuery] bool? hasFollowUp = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.Enquiries.AsNoTracking().AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status != null && x.Status.Trim().ToLower() == status.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(priority)) query = query.Where(x => x.Priority != null && x.Priority.Trim().ToLower() == priority.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(source)) query = query.Where(x => x.Source != null && x.Source.ToLower().Contains(source.Trim().ToLower()));
            if (hasFollowUp == true) query = query.Where(x => x.FollowUpDate != null);
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
            query = query.OrderByDescending(x => x.EnquiryDate);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [Authorize(Roles = "Admin,Staff,Receptionist,Principal")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.Enquiries.FindAsync(id);
            return entity == null ? NotFound(new { message = "Enquiry not found" }) : Ok(entity);
        }

        [Authorize(Roles = "Admin,Staff,Receptionist")]
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

        [Authorize(Roles = "Admin,Staff")]
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

        [Authorize(Roles = "Admin,Staff")]
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
