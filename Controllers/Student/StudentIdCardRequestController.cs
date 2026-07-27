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
    [Route("api/student-id-card-requests")]
    public class StudentIdCardRequestController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IStudentIdCardRequestManager _manager;
        private readonly INotificationOrchestrator? _notify;

        public StudentIdCardRequestController(ApplicationDbContext context, IStudentIdCardRequestManager manager, INotificationOrchestrator? notify = null)
        {
            _context = context;
            _manager = manager;
            _notify = notify;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? academicYearId = null, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.StudentIdCardRequests.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.CardNumber != null && x.CardNumber.ToLower().Contains(term)) ||
                    (x.ChipUid != null && x.ChipUid.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(term)) ||
                    (x.IssueReason != null && x.IssueReason.ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.RequestDate).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? academicYearId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = _context.StudentIdCardRequests.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.CardNumber != null && x.CardNumber.ToLower().Contains(term)) ||
                    (x.ChipUid != null && x.ChipUid.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(term)) ||
                    (x.IssueReason != null && x.IssueReason.ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.RequestDate);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _context.StudentIdCardRequests.FindAsync(id);
            return entity == null ? NotFound(new { message = "Student ID card request not found" }) : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StudentIdCardRequest entity)
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
            if (!entity.RequestedByUserId.HasValue)
                entity.RequestedByUserId = GetCurrentUserId();
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] StudentIdCardRequest entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _context.StudentIdCardRequests.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Student ID card request not found" });
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
            var existing = await _context.StudentIdCardRequests.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Student ID card request not found" });
            if (!existing.IsActive) return BadRequest(new { message = "Student ID card request is inactive" });
            existing.Status = "Approved";
            existing.ApprovedAt = DateTime.UtcNow;
            existing.ApprovedByUserId = GetCurrentUserId();
            await _context.SaveChangesAsync();
            await NotifyApprovedAsync(existing, "Pending");
            return Ok(existing);
        }

        private async Task NotifyApprovedAsync(StudentIdCardRequest request, string? priorStatus)
        {
            if (!string.Equals(request.Status, "Approved", StringComparison.OrdinalIgnoreCase)) return;
            if (string.Equals(priorStatus, "Approved", StringComparison.OrdinalIgnoreCase)) return;

            var recipients = new List<Guid>();
            if (request.StudentId.HasValue)
            {
                var studentUserId = await NotificationEvents.GetStudentUserIdAsync(_context, request.StudentId.Value);
                if (studentUserId.HasValue) recipients.Add(studentUserId.Value);
            }
            if (request.RequestedByUserId.HasValue)
                recipients.Add(request.RequestedByUserId.Value);

            await NotificationEvents.NotifyUsersAsync(
                _notify,
                recipients.Distinct(),
                "ID card request approved",
                "Your student ID card request has been approved.",
                "idcard.approved",
                "approvals",
                NotificationChannels.All,
                request.Id,
                "StudentIdCardRequest",
                "success",
                idempotencyKey: $"idcard.approved:{request.Id}");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.StudentIdCardRequests.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Student ID card request not found" });
            existing.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
