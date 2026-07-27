using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Controllers.Shared;
using sms.api.Data;
using sms.api.Interfaces.Student;
using sms.api.Models;
using sms.api.Services;
using sms.api.Utils;

namespace sms.api.Controllers.Student
{
    [Authorize(Roles = "Admin,Staff")]
    [Route("api/scholarship-applications")]
    public class ScholarshipApplicationController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IScholarshipApplicationManager _manager;
        private readonly INotificationOrchestrator? _notify;

        public ScholarshipApplicationController(ApplicationDbContext context, IScholarshipApplicationManager manager, INotificationOrchestrator? notify = null)
        {
            _context = context;
            _manager = manager;
            _notify = notify;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? academicYearId = null, [FromQuery] Guid? scholarshipId = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.ScholarshipApplications.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (scholarshipId.HasValue) query = query.Where(x => x.ScholarshipId == scholarshipId.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            var items = await query.OrderByDescending(x => x.AppliedOn).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? academicYearId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? scholarshipId = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.ScholarshipApplications.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (scholarshipId.HasValue) query = query.Where(x => x.ScholarshipId == scholarshipId.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            query = query.OrderByDescending(x => x.AppliedOn);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.ScholarshipApplications.FindAsync(id);
            return entity == null ? NotFound(new { message = "Scholarship application not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ScholarshipApplication entity)
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
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ScholarshipApplication entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.ScholarshipApplications.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Scholarship application not found" });
            entity.Id = id;
            if (entity.AcademicYearId == default) entity.AcademicYearId = existing.AcademicYearId;
            var priorStatus = existing.Status;
            if (string.Equals(entity.Status, "Approved", StringComparison.OrdinalIgnoreCase) && !existing.ApprovedAt.HasValue)
            {
                entity.ApprovedAt = DateTime.UtcNow;
                entity.ApprovedByUserId = GetCurrentUserId();
            }
            var updated = await _manager.UpdateAsync(entity);
            await NotifyApprovedAsync(updated, priorStatus);
            return Ok(updated);
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var existing = await _context.ScholarshipApplications.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Scholarship application not found" });
            if (!existing.IsActive) return BadRequest(new { message = "Scholarship application is inactive" });
            existing.Status = "Approved";
            existing.ApprovedAt = DateTime.UtcNow;
            existing.ApprovedByUserId = GetCurrentUserId();
            await _context.SaveChangesAsync();
            await NotifyApprovedAsync(existing, "Pending");
            return Ok(existing);
        }

        private async Task NotifyApprovedAsync(ScholarshipApplication app, string? priorStatus)
        {
            if (!string.Equals(app.Status, "Approved", StringComparison.OrdinalIgnoreCase)) return;
            if (string.Equals(priorStatus, "Approved", StringComparison.OrdinalIgnoreCase)) return;
            if (!app.StudentId.HasValue) return;

            var studentUserId = await NotificationEvents.GetStudentUserIdAsync(_context, app.StudentId.Value);
            if (!studentUserId.HasValue) return;

            await NotificationEvents.NotifyUsersAsync(
                _notify,
                new[] { studentUserId.Value },
                "Scholarship application approved",
                "Your scholarship application has been approved.",
                "scholarship.approved",
                "approvals",
                NotificationChannels.All,
                app.Id,
                "ScholarshipApplication",
                "success",
                idempotencyKey: $"scholarship.approved:{app.Id}");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.ScholarshipApplications.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Scholarship application not found" });
            existing.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
