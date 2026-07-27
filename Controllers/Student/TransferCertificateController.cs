using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Controllers.Shared;
using sms.api.Data;
using sms.api.Interfaces.Student;
using sms.api.Models;
using sms.api.Models.DTOs;
using sms.api.Services;
using sms.api.Utils;

namespace sms.api.Controllers.Student
{
    [Authorize(Roles = "Admin,Staff")]
    [Route("api/transfer-certificates")]
    public class TransferCertificateController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITransferCertificateManager _manager;
        private readonly INotificationOrchestrator? _notify;

        public TransferCertificateController(ApplicationDbContext context, ITransferCertificateManager manager, INotificationOrchestrator? notify = null)
        {
            _context = context;
            _manager = manager;
            _notify = notify;
        }

        private static IQueryable<TransferCertificate> ApplyIncludes(IQueryable<TransferCertificate> q) =>
            q.Include(t => t.Student).ThenInclude(s => s.User);

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? academicYearId = null, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = ApplyIncludes(_context.TransferCertificates.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value));
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.TcNumber != null && x.TcNumber.ToLower().Contains(term)) ||
                    (x.Reason != null && x.Reason.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)) ||
                    (x.Remarks != null && x.Remarks.ToLower().Contains(term)) ||
                    (x.Student != null && x.Student.User != null && (x.Student.User.FirstName + " " + x.Student.User.LastName).ToLower().Contains(term)));
            }
            var items = await query.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] Guid? academicYearId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] bool includeInactive = false)
        {
            var yearId = academicYearId ?? GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var query = ApplyIncludes(_context.TransferCertificates.AsNoTracking().Where(x => x.AcademicYearId == yearId.Value));
            if (!includeInactive) query = query.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.TcNumber != null && x.TcNumber.ToLower().Contains(term)) ||
                    (x.Reason != null && x.Reason.ToLower().Contains(term)) ||
                    (x.Status != null && x.Status.ToLower().Contains(term)) ||
                    (x.Remarks != null && x.Remarks.ToLower().Contains(term)) ||
                    (x.Student != null && x.Student.User != null && (x.Student.User.FirstName + " " + x.Student.User.LastName).ToLower().Contains(term)));
            }
            query = query.OrderByDescending(x => x.CreatedAt);
            return Ok(await query.ToPagedResultAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await ApplyIncludes(_context.TransferCertificates.AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync();
            return entity == null ? NotFound(new { message = "Transfer certificate not found" }) : Ok(entity);
        }

        private static TransferCertificate MapDtoToEntity(TransferCertificateCreateUpdateDto dto, TransferCertificate entity)
        {
            entity.StudentId = dto.StudentId;
            entity.TcNumber = dto.TcNumber;
            entity.IssueDate = dto.IssueDate;
            entity.Reason = dto.ReasonForLeaving;
            entity.AdmissionDate = dto.AdmissionDate;
            entity.LeavingDate = dto.LeavingDate;
            entity.LastClassStudied = dto.LastClassStudied;
            entity.PromotedToClass = dto.PromotedToClass;
            entity.SubjectsStudied = dto.SubjectsStudied;
            entity.CharacterConduct = dto.CharacterConduct;
            entity.AcademicPerformance = dto.AcademicPerformance;
            entity.AnyDuesPending = dto.AnyDuesPending;
            entity.LibraryClearance = dto.LibraryClearance;
            entity.LabClearance = dto.LabClearance;
            entity.TransportClearance = dto.TransportClearance;
            if (!string.IsNullOrWhiteSpace(dto.Status)) entity.Status = dto.Status;
            entity.Remarks = dto.Remarks;
            return entity;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransferCertificateCreateUpdateDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body required" });
            var yearId = GetCurrentAcademicYearId();
            if (!yearId.HasValue) return BadRequest(new { message = "Academic year not selected" });

            var entity = new TransferCertificate
            {
                Id = Guid.NewGuid(),
                AcademicYearId = yearId.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                IsActive = true,
                GeneratedByUserId = GetCurrentUserId(),
                Status = dto.Status ?? "Pending"
            };
            MapDtoToEntity(dto, entity);
            var created = await _manager.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, await ApplyIncludes(_context.TransferCertificates.AsNoTracking().Where(x => x.Id == created.Id)).FirstOrDefaultAsync());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TransferCertificateCreateUpdateDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body required" });
            var existing = await _context.TransferCertificates.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Transfer certificate not found" });

            var priorStatus = existing.Status;
            MapDtoToEntity(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            if (string.Equals(dto.Status, "Approved", StringComparison.OrdinalIgnoreCase) && !existing.ApprovedAt.HasValue)
            {
                existing.ApprovedAt = DateTime.UtcNow;
                existing.ApprovedByUserId = GetCurrentUserId();
            }
            await _context.SaveChangesAsync();
            await NotifyStatusChangeAsync(existing, priorStatus);
            return Ok(await ApplyIncludes(_context.TransferCertificates.AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync());
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var existing = await _context.TransferCertificates.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Transfer certificate not found" });
            if (!existing.IsActive) return BadRequest(new { message = "Transfer certificate is inactive" });
            existing.Status = "Approved";
            existing.ApprovedAt = DateTime.UtcNow;
            existing.ApprovedByUserId = GetCurrentUserId();
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await NotifyStatusChangeAsync(existing, "Pending");
            return Ok(existing);
        }

        private async Task NotifyStatusChangeAsync(TransferCertificate tc, string? priorStatus)
        {
            if (tc.StudentId == null) return;
            if (string.Equals(tc.Status, priorStatus, StringComparison.OrdinalIgnoreCase)) return;

            var studentUserId = await NotificationEvents.GetStudentUserIdAsync(_context, tc.StudentId.Value);
            if (!studentUserId.HasValue) return;

            if (string.Equals(tc.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                await NotificationEvents.NotifyUsersAsync(
                    _notify,
                    new[] { studentUserId.Value },
                    "Transfer certificate approved",
                    $"Your transfer certificate ({tc.TcNumber ?? "TC"}) has been approved.",
                    "tc.approved",
                    "approvals",
                    NotificationChannels.All,
                    tc.Id,
                    "TransferCertificate",
                    "success",
                    idempotencyKey: $"tc.approved:{tc.Id}");
            }
            else if (string.Equals(tc.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                await NotificationEvents.NotifyUsersAsync(
                    _notify,
                    new[] { studentUserId.Value },
                    "Transfer certificate rejected",
                    $"Your transfer certificate request was rejected. {tc.Remarks}".Trim(),
                    "tc.rejected",
                    "approvals",
                    NotificationChannels.All,
                    tc.Id,
                    "TransferCertificate",
                    "warning",
                    idempotencyKey: $"tc.rejected:{tc.Id}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.TransferCertificates.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Transfer certificate not found" });
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deactivated" });
        }
    }
}
