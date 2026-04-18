using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models.DTOs;
using sms.api.Models.Enums;
using System.Security.Claims;

namespace sms.api.Controllers.Student
{
    [Route("api/student-status")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StudentStatusController : ControllerBase
    {
        private readonly IStudentManager _studentManager;

        public StudentStatusController(IStudentManager studentManager)
        {
            _studentManager = studentManager;
        }

        [HttpPost("change-status")]
        public async Task<IActionResult> ChangeStudentStatus([FromBody] ChangeStudentStatusDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var student = await _studentManager.GetByIdAsync(dto.StudentId);
                if (student == null)
                    return NotFound(new { message = "Student not found" });

                // Get current user ID for audit trail
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserId, out var changedBy))
                    return BadRequest(new { message = "Invalid user context" });

                // Update student status
                student.Status = dto.NewStatus;
                student.StatusChangeReason = dto.Reason;
                student.StatusChangedAt = DateTime.UtcNow;
                student.StatusChangedBy = changedBy;
                student.UpdatedAt = DateTime.UtcNow;

                var updatedStudent = await _studentManager.UpdateAsync(student);

                return Ok(new 
                { 
                    message = $"Student status changed to {dto.NewStatus.GetDisplayName()} successfully",
                    studentId = student.Id,
                    newStatus = dto.NewStatus,
                    statusDisplayName = dto.NewStatus.GetDisplayName(),
                    changedAt = student.StatusChangedAt,
                    reason = dto.Reason
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error changing student status", error = ex.Message });
            }
        }

        [HttpGet("status-summary")]
        public async Task<IActionResult> GetStatusSummary()
        {
            try
            {
                var allStudents = await _studentManager.GetAllAsync();
                
                var summary = new StudentStatusSummaryDTO
                {
                    TotalStudents = allStudents.Count(),
                    ActiveStudents = allStudents.Count(s => s.Status == StudentStatus.Active),
                    InactiveStudents = allStudents.Count(s => s.Status == StudentStatus.Inactive),
                    PassOutStudents = allStudents.Count(s => s.Status == StudentStatus.PassOut),
                    LeftStudents = allStudents.Count(s => s.Status == StudentStatus.Left)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting status summary", error = ex.Message });
            }
        }

        [HttpGet("status-history")]
        public async Task<IActionResult> GetStatusHistory([FromQuery] Guid? studentId = null)
        {
            try
            {
                var students = studentId.HasValue 
                    ? new[] { await _studentManager.GetByIdAsync(studentId.Value) }.Where(s => s != null)
                    : await _studentManager.GetAllAsync();

                var history = students.Where(s => s.StatusChangedAt.HasValue)
                    .Select(s => new StudentStatusHistoryDTO
                    {
                        StudentId = s.Id,
                        StudentName = $"{s.User?.FirstName} {s.User?.LastName}".Trim(),
                        RegistrationNumber = s.RegistrationNumber,
                        CurrentStatus = s.Status,
                        StatusChangedAt = s.StatusChangedAt,
                        StatusChangeReason = s.StatusChangeReason,
                        StatusChangedBy = s.StatusChangedBy,
                        StatusChangedByName = s.StatusChangedByUser != null 
                            ? $"{s.StatusChangedByUser.FirstName} {s.StatusChangedByUser.LastName}".Trim()
                            : "System"
                    })
                    .OrderByDescending(h => h.StatusChangedAt)
                    .ToList();

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting status history", error = ex.Message });
            }
        }

        [HttpGet("by-status/{status}")]
        public async Task<IActionResult> GetStudentsByStatus(StudentStatus status)
        {
            try
            {
                var allStudents = await _studentManager.GetAllAsync();
                var filteredStudents = allStudents.Where(s => s.Status == status);

                var result = filteredStudents.Select(s => new
                {
                    s.Id,
                    FullName = $"{s.User?.FirstName} {s.User?.LastName}".Trim(),
                    s.RegistrationNumber,
                    s.RollNumber,
                    ClassName = s.Class?.Name ?? s.CurrentGrade ?? "No Class",
                    Status = s.Status,
                    StatusDisplayName = s.Status.GetDisplayName(),
                    StatusChangedAt = s.StatusChangedAt,
                    StatusChangeReason = s.StatusChangeReason,
                    ProfilePicture = s.User?.ProfilePhotoData != null && s.User.ProfilePhotoData.Length > 0
                        ? Convert.ToBase64String(s.User.ProfilePhotoData)
                        : null
                }).ToList();

                return Ok(new 
                { 
                    status = status,
                    statusDisplayName = status.GetDisplayName(),
                    count = result.Count,
                    students = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting students by status", error = ex.Message });
            }
        }

        [HttpGet("class-count/{classId}")]
        public async Task<IActionResult> GetClassStudentCount(Guid classId, [FromQuery] bool activeOnly = true)
        {
            try
            {
                var allStudents = await _studentManager.GetAllAsync();
                var classStudents = allStudents.Where(s => s.ClassId == classId);

                if (activeOnly)
                {
                    classStudents = classStudents.Where(s => s.Status.IsCountedInClass());
                }

                var count = classStudents.Count();
                var statusBreakdown = classStudents.GroupBy(s => s.Status)
                    .Select(g => new 
                    {
                        Status = g.Key,
                        StatusDisplayName = g.Key.GetDisplayName(),
                        Count = g.Count()
                    }).ToList();

                return Ok(new 
                { 
                    classId,
                    totalCount = count,
                    activeOnly,
                    statusBreakdown
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting class student count", error = ex.Message });
            }
        }

        [HttpPost("bulk-status-change")]
        public async Task<IActionResult> BulkStatusChange([FromBody] BulkStatusChangeDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserId, out var changedBy))
                    return BadRequest(new { message = "Invalid user context" });

                var updatedCount = 0;
                var errors = new List<string>();

                foreach (var studentId in dto.StudentIds)
                {
                    try
                    {
                        var student = await _studentManager.GetByIdAsync(studentId);
                        if (student == null)
                        {
                            errors.Add($"Student with ID {studentId} not found");
                            continue;
                        }

                        student.Status = dto.NewStatus;
                        student.StatusChangeReason = dto.Reason;
                        student.StatusChangedAt = DateTime.UtcNow;
                        student.StatusChangedBy = changedBy;
                        student.UpdatedAt = DateTime.UtcNow;

                        await _studentManager.UpdateAsync(student);
                        updatedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error updating student {studentId}: {ex.Message}");
                    }
                }

                return Ok(new 
                { 
                    message = $"Bulk status change completed. {updatedCount} students updated.",
                    updatedCount,
                    totalRequested = dto.StudentIds.Count,
                    newStatus = dto.NewStatus,
                    statusDisplayName = dto.NewStatus.GetDisplayName(),
                    errors = errors.Any() ? errors : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error performing bulk status change", error = ex.Message });
            }
        }
    }

    public class BulkStatusChangeDTO
    {
        public List<Guid> StudentIds { get; set; } = new();
        public StudentStatus NewStatus { get; set; }
        public string? Reason { get; set; }
    }
}
