using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Utils;

namespace sms.api.Controllers.Shared
{
    // NOTE: intentionally extends ApiControllerBase (no class-level role restriction) rather than
    // ExtraModulesControllerBase ([Authorize(Roles = "Admin,Staff")]). ASP.NET Core combines a
    // class-level [Authorize(Roles=...)] with any method-level [Authorize(Roles=...)] using AND
    // semantics (the user must satisfy both role sets), so a derived class/action can only ever
    // narrow the base's "Admin,Staff" - it can never add Parent/Teacher/etc. ApiControllerBase
    // exposes the same GetCurrentAcademicYearId()/GetDataScopeAsync() helpers without that
    // restriction, letting each action below declare its own correct role list.
    [Route("api/homework-assignments")]
    public class HomeworkAssignmentController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHomeworkAssignmentManager _manager;

        public HomeworkAssignmentController(ApplicationDbContext context, IHomeworkAssignmentManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff,Parent")]
        public async Task<IActionResult> GetAll([FromQuery] Guid? academicYearId = null, [FromQuery] string? searchTerm = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var scope = await GetDataScopeAsync();
            var query = _context.HomeworkAssignments.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
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
                    (x.Title != null && x.Title.ToLower().Contains(term)) ||
                    (x.Description != null && x.Description.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin,Staff,Parent")]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? academicYearId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] string? tab = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var scope = await GetDataScopeAsync();
            var query = _context.HomeworkAssignments.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);

            var allowedClassIds = DataScopeHelper.AllowedClassIdsForQuery(scope);
            if (allowedClassIds != null)
            {
                var allowed = allowedClassIds.ToList();
                query = query.Where(x => x.ClassId.HasValue && allowed.Contains(x.ClassId.Value));
            }

            if (!string.IsNullOrWhiteSpace(tab))
            {
                if (string.Equals(tab, "Active", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => x.Status == "Draft" || x.Status == "Published");
                else if (string.Equals(tab, "History", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => x.Status == "Closed" || x.Status == "Evaluated");
            }
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status != null && x.Status.ToLower() == status.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(term)) ||
                    (x.Description != null && x.Description.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.CreatedAt);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Parent")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.HomeworkAssignments.FindAsync(id);
            if (entity == null) return NotFound(new { message = "Homework assignment not found" });

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted)
            {
                if (!entity.ClassId.HasValue || !scope.CanAccessClass(entity.ClassId.Value))
                    return Forbid();
            }

            return Ok(entity);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] HomeworkAssignment entity)
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
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] HomeworkAssignment entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.HomeworkAssignments.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Homework assignment not found" });

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
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.HomeworkAssignments.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Homework assignment not found" });

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
