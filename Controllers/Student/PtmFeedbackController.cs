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
    [Route("api/ptm-feedback")]
    public class PtmFeedbackController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPtmFeedbackManager _manager;

        public PtmFeedbackController(ApplicationDbContext context, IPtmFeedbackManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? academicYearId = null, [FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.PtmFeedbacks.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.ParentName != null && x.ParentName.ToLower().Contains(term)) ||
                    (x.Comments != null && x.Comments.ToLower().Contains(term)) ||
                    (x.MeetingTopic != null && x.MeetingTopic.ToLower().Contains(term)) ||
                    (x.SpecificStrengths != null && x.SpecificStrengths.ToLower().Contains(term)) ||
                    (x.AreasForImprovement != null && x.AreasForImprovement.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.FeedbackDate).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? academicYearId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.PtmFeedbacks.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.ParentName != null && x.ParentName.ToLower().Contains(term)) ||
                    (x.Comments != null && x.Comments.ToLower().Contains(term)) ||
                    (x.MeetingTopic != null && x.MeetingTopic.ToLower().Contains(term)) ||
                    (x.SpecificStrengths != null && x.SpecificStrengths.ToLower().Contains(term)) ||
                    (x.AreasForImprovement != null && x.AreasForImprovement.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.FeedbackDate);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.PtmFeedbacks.FindAsync(id);
            return entity == null ? NotFound(new { message = "PTM feedback not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PtmFeedback entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            entity.Id = Guid.NewGuid();
            if (entity.AcademicYearId == default)
            {
                var yearId = GetCurrentAcademicYearId();
                if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });
                entity.AcademicYearId = yearId.Value;
            }
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PtmFeedback entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.PtmFeedbacks.FindAsync(id);
            if (existing == null) return NotFound(new { message = "PTM feedback not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            if (entity.AcademicYearId == default) entity.AcademicYearId = existing.AcademicYearId;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.PtmFeedbacks.FindAsync(id);
            if (existing == null) return NotFound(new { message = "PTM feedback not found" });
            existing.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
