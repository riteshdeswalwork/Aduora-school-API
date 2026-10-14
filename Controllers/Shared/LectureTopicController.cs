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

            var scope = await GetDataScopeAsync();
            var query = _context.LectureTopics.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);

            var allowedClassIds = DataScopeHelper.AllowedClassIdsForQuery(scope);
            if (allowedClassIds != null)
            {
                var allowed = allowedClassIds.ToList();
                query = query.Where(x => x.ClassId.HasValue && allowed.Contains(x.ClassId.Value));
            }

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

            var scope = await GetDataScopeAsync();
            var allowedClassIds = DataScopeHelper.AllowedClassIdsForQuery(scope);

            var result = await _manager.GetPagedAsync(yearId.Value, pageNumber, pageSize, searchTerm, status, tab, includeInactive, allowedClassIds);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.LectureTopics.FindAsync(id);
            if (entity == null) return NotFound(new { message = "Lecture topic not found" });

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted)
            {
                if (!entity.ClassId.HasValue || !scope.CanAccessClass(entity.ClassId.Value))
                    return Forbid();
            }

            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LectureTopic entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted)
            {
                if (!entity.ClassId.HasValue || !scope.CanAccessClass(entity.ClassId.Value))
                    return Forbid();
            }

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

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted)
            {
                if (!existing.ClassId.HasValue || !scope.CanAccessClass(existing.ClassId.Value))
                    return Forbid();
                if (entity.ClassId.HasValue && !scope.CanAccessClass(entity.ClassId.Value))
                    return Forbid();
            }

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

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted)
            {
                if (!existing.ClassId.HasValue || !scope.CanAccessClass(existing.ClassId.Value))
                    return Forbid();
            }

            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
