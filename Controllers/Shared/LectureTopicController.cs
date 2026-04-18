using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    [Route("api/lecture-topics")]
    public class LectureTopicController : ExtraModulesControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILectureTopicManager _manager;

        public LectureTopicController(ApplicationDbContext context, ILectureTopicManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? academicYearId = null, [FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.LectureTopics.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Topic != null && x.Topic.ToLower().Contains(term)) ||
                    (x.Details != null && x.Details.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.LectureDate).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? academicYearId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] string? tab = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.LectureTopics.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(tab))
            {
                if (string.Equals(tab, "Planned", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => x.Status == "Planned" || x.Status == "Postponed");
                else if (string.Equals(tab, "Covered", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => x.Status == "Covered");
            }
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status != null && x.Status.ToLower() == status.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Topic != null && x.Topic.ToLower().Contains(term)) ||
                    (x.Details != null && x.Details.ToLower().Contains(term)) ||
                    (x.ChapterOrUnit != null && x.ChapterOrUnit.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.LectureDate);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.LectureTopics.FindAsync(id);
            return entity == null ? NotFound(new { message = "Lecture topic not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LectureTopic entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            entity.Id = Guid.NewGuid();
            if (entity.AcademicYearId == default)
            {
                var yearId = GetCurrentAcademicYearId();
                if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });
                entity.AcademicYearId = yearId.Value;
            }
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = null;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LectureTopic entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.LectureTopics.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Lecture topic not found" });
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            if (entity.AcademicYearId == default) entity.AcademicYearId = existing.AcademicYearId;
            var updated = await _manager.UpdateAsync(entity);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.LectureTopics.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Lecture topic not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
