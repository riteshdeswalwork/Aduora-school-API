using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Controllers.Student
{
    [Authorize]
    public class StudentLeaveController : ApiControllerBase
    {
        private readonly IStudentLeaveManager _studentLeaveManager;

        public StudentLeaveController(IStudentLeaveManager studentLeaveManager)
        {
            _studentLeaveManager = studentLeaveManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var leaves = await _studentLeaveManager.GetAllAsync();
            var mappedLeaves = leaves.Select(MapToResponseDTO);
            return Ok(mappedLeaves);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var leave = await _studentLeaveManager.GetByIdAsync(id);
            if (leave == null)
                return NotFound();

            if (User.IsInRole("Student"))
            {
                var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentIdClaim) || leave.StudentId.ToString() != studentIdClaim)
                    return Forbid();
            }
            else
            {
                var scope = await GetDataScopeAsync();
                if (!scope.IsUnrestricted)
                {
                    if (!leave.ClassId.HasValue || !scope.CanAccessClass(leave.ClassId.Value))
                        return Forbid();
                }
            }

            return Ok(MapToResponseDTO(leave));
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(Guid studentId)
        {
            if (User.IsInRole("Student"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || studentId.ToString() != userIdClaim)
                    return Forbid();
            }
            else
            {
                var scope = await GetDataScopeAsync();
                if (!scope.IsUnrestricted && !scope.CanAccessStudent(studentId))
                    return Forbid();
            }

            var leaves = await _studentLeaveManager.GetLeavesByStudentIdAsync(studentId);
            var mappedLeaves = leaves.Select(MapToResponseDTO);
            return Ok(mappedLeaves);
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByClass(Guid classId)
        {
            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessClass(classId))
                return Forbid();

            var leaves = await _studentLeaveManager.GetLeavesByClassIdAsync(classId);
            var mappedLeaves = leaves.Select(MapToResponseDTO);
            return Ok(mappedLeaves);
        }

        [HttpGet("pending/teacher")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetPendingLeavesForTeacher()
        {
            var scope = await GetDataScopeAsync();
            if (!scope.StaffId.HasValue)
                return Forbid();

            var leaves = await _studentLeaveManager.GetPendingLeavesForTeacherAsync(scope.StaffId.Value);
            if (!scope.IsUnrestricted)
            {
                var allowed = scope.AllowedClassIds?.ToHashSet() ?? new HashSet<Guid>();
                leaves = leaves.Where(l => l.ClassId.HasValue && allowed.Contains(l.ClassId.Value));
            }

            var mappedLeaves = leaves.Select(MapToResponseDTO);
            return Ok(mappedLeaves);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create([FromBody] CreateStudentLeaveDTO leaveDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studentIdClaim) || leaveDto.StudentId.ToString() != studentIdClaim)
                return Forbid();

            var leave = await _studentLeaveManager.CreateStudentLeaveAsync(leaveDto);
            return CreatedAtAction(nameof(GetById), new { id = leave.Id }, MapToResponseDTO(leave));
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var leave = await _studentLeaveManager.GetByIdAsync(id);
            if (leave == null)
                return NotFound();

            var approverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted)
            {
                if (!leave.ClassId.HasValue)
                    return BadRequest(new { message = "Leave is not associated with a class" });
                if (!scope.CanAccessClass(leave.ClassId.Value))
                    return Forbid();
            }

            var result = await _studentLeaveManager.ApproveLeaveAsync(id, approverId);
            if (!result)
                return BadRequest(new { message = "Failed to approve leave" });

            return Ok(new { message = "Leave approved successfully" });
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLeaveDTO rejectDto)
        {
            var leave = await _studentLeaveManager.GetByIdAsync(id);
            if (leave == null)
                return NotFound();

            var approverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted)
            {
                if (!leave.ClassId.HasValue)
                    return BadRequest(new { message = "Leave is not associated with a class" });
                if (!scope.CanAccessClass(leave.ClassId.Value))
                    return Forbid();
            }

            var result = await _studentLeaveManager.RejectLeaveAsync(id, approverId, rejectDto.Reason);
            if (!result)
                return BadRequest(new { message = "Failed to reject leave" });

            return Ok(new { message = "Leave rejected successfully" });
        }

        private StudentLeaveResponseDTO MapToResponseDTO(StudentLeave leave)
        {
            return new StudentLeaveResponseDTO
            {
                Id = leave.Id,
                StudentId = leave.StudentId,
                StudentName = leave.Student?.User?.FullName ?? string.Empty,
                ClassId = leave.ClassId,
                ClassName = leave.Class?.Name ?? string.Empty,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status,
                RejectionReason = leave.RejectionReason,
                CreatedAt = leave.CreatedAt,
                UpdatedAt = leave.UpdatedAt,
                ApprovedById = leave.ApprovedById,
                ApprovedByName = leave.ApprovedBy?.FullName ?? string.Empty,
                ApprovedAt = leave.ApprovedAt
            };
        }
    }

    public class RejectLeaveDTO
    {
        public string Reason { get; set; } = string.Empty;
    }
}
