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
    [Route("api/student-behavior-incidents")]
    public class StudentBehaviorIncidentController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IStudentBehaviorIncidentManager _manager;

        public StudentBehaviorIncidentController(ApplicationDbContext context, IStudentBehaviorIncidentManager manager)
        {
            _context = context;
            _manager = manager;
        }

        private static IQueryable<StudentBehaviorIncident> ApplyIncludes(IQueryable<StudentBehaviorIncident> q)
        {
            return q.Include(x => x.Student).ThenInclude(s => s!.User)
                .Include(x => x.Class);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? academicYearId = null, [FromQuery] Guid? studentId = null, [FromQuery] Guid? classId = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] int? severityIndex = null, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.StudentBehaviorIncidents.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (studentId.HasValue) query = query.Where(x => x.StudentId == studentId.Value);
            if (classId.HasValue) query = query.Where(x => x.ClassId == classId.Value);
            if (fromDate.HasValue) query = query.Where(x => x.IncidentDate >= fromDate.Value);
            if (toDate.HasValue) { var end = toDate.Value.Date.AddDays(1); query = query.Where(x => x.IncidentDate < end); }
            if (severityIndex.HasValue) query = query.Where(x => x.SeverityIndex == severityIndex.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.BehaviorType != null && x.BehaviorType.ToLower().Contains(term)) ||
                    (x.Description != null && x.Description.ToLower().Contains(term)) ||
                    (x.InterventionPlan != null && x.InterventionPlan.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)) ||
                    (x.LocationOfIncident != null && x.LocationOfIncident.ToLower().Contains(term)) ||
                    (x.ActionTaken != null && x.ActionTaken.ToLower().Contains(term)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(term)));
            }
            query = ApplyIncludes(query).OrderByDescending(x => x.IncidentDate);
            var items = await query.Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? academicYearId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? studentId = null, [FromQuery] Guid? classId = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] int? severityIndex = null, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var result = await _manager.GetPagedAsync(
                yearId.Value, pageNumber, pageSize, studentId, classId, fromDate, toDate,
                severityIndex, searchTerm, status, includeInactive);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await ApplyIncludes(_context.StudentBehaviorIncidents.AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync();
            return entity == null ? NotFound(new { message = "Student behavior incident not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StudentBehaviorIncident entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            entity.Id = Guid.NewGuid();
            if (entity.AcademicYearId == default)
            {
                var yearId = GetCurrentAcademicYearId();
                if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });
                entity.AcademicYearId = yearId.Value;
            }
            entity.ReportedByUserId = GetCurrentUserId();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = null;
            entity.IsActive = true;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] StudentBehaviorIncident entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.StudentBehaviorIncidents.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Student behavior incident not found" });
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
            var existing = await _context.StudentBehaviorIncidents.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Student behavior incident not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
