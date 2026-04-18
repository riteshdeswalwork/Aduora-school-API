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
    [Route("api/achievement-talents")]
    public class AchievementTalentController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAchievementTalentManager _manager;

        public AchievementTalentController(ApplicationDbContext context, IAchievementTalentManager manager)
        {
            _context = context;
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? academicYearId = null, [FromQuery] string? searchTerm = null, [FromQuery] string? type = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.AchievementTalents.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(type))
            {
                var typeNorm = type.Trim();
                query = query.Where(x => x.Type != null && x.Type.ToLower() == typeNorm.ToLower());
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(term)) ||
                    (x.Category != null && x.Category.ToLower().Contains(term)) ||
                    (x.Level != null && x.Level.ToLower().Contains(term)) ||
                    (x.Description != null && x.Description.ToLower().Contains(term)) ||
                    (x.AwardedBy != null && x.AwardedBy.ToLower().Contains(term)) ||
                    (x.EventName != null && x.EventName.ToLower().Contains(term)) ||
                    (x.Position != null && x.Position.ToLower().Contains(term)) ||
                    (x.Remarks != null && x.Remarks.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.AchievementDate).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? academicYearId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? type = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.AchievementTalents.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(type))
            {
                var typeNorm = type.Trim();
                query = query.Where(x => x.Type != null && x.Type.ToLower() == typeNorm.ToLower());
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(term)) ||
                    (x.Category != null && x.Category.ToLower().Contains(term)) ||
                    (x.Level != null && x.Level.ToLower().Contains(term)) ||
                    (x.Description != null && x.Description.ToLower().Contains(term)) ||
                    (x.AwardedBy != null && x.AwardedBy.ToLower().Contains(term)) ||
                    (x.EventName != null && x.EventName.ToLower().Contains(term)) ||
                    (x.Position != null && x.Position.ToLower().Contains(term)) ||
                    (x.Remarks != null && x.Remarks.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.AchievementDate);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.AchievementTalents.FindAsync(id);
            return entity == null ? NotFound(new { message = "Achievement/talent not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AchievementTalent entity)
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
            entity.UpdatedAt = null;
            entity.IsActive = true;
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AchievementTalent entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.AchievementTalents.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Achievement/talent not found" });
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
            var existing = await _context.AchievementTalents.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Achievement/talent not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
