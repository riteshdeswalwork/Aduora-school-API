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
        private readonly IClassManager _classManager;

        public StudentLeaveController(IStudentLeaveManager studentLeaveManager, IClassManager classManager)
        {
            _studentLeaveManager = studentLeaveManager;
            _classManager = classManager;
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

            // If student, only allow access to own leaves
            if (User.IsInRole("Student"))
            {
                var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentIdClaim) || leave.StudentId.ToString() != studentIdClaim)
                    return Forbid();
            }
            // If staff, only allow access to leaves from their classes
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var classId = leave.ClassId;

                if (!classId.HasValue)
                    return Forbid();

                var classInfo = await _classManager.GetClassByIdAsync(classId.Value);
                if (classInfo == null || classInfo.TeacherId != staffId)
                    return Forbid();
            }

            return Ok(MapToResponseDTO(leave));
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(Guid studentId)
        {
            // If student, only allow access to own leaves
            if (User.IsInRole("Student"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || studentId.ToString() != userIdClaim)
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
            // If staff, only allow access to their assigned classes
            if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var classInfo = await _classManager.GetClassByIdAsync(classId);
                
                if (classInfo == null || classInfo.TeacherId != staffId)
                    return Forbid();
            }

            var leaves = await _studentLeaveManager.GetLeavesByClassIdAsync(classId);
            var mappedLeaves = leaves.Select(MapToResponseDTO);
            return Ok(mappedLeaves);
        }

        [HttpGet("pending/teacher")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetPendingLeavesForTeacher()
        {
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim))
                return Forbid();

            var staffId = Guid.Parse(staffIdClaim);
            var leaves = await _studentLeaveManager.GetPendingLeavesForTeacherAsync(staffId);
            var mappedLeaves = leaves.Select(MapToResponseDTO);
            return Ok(mappedLeaves);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create([FromBody] CreateStudentLeaveDTO leaveDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ensure the student is only creating a leave for themselves
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

            // If staff, check if they are the teacher of the student's class
            if (User.IsInRole("Staff"))
            {
                var classId = leave.ClassId;
                if (!classId.HasValue)
                    return BadRequest(new { message = "Leave is not associated with a class" });

                var classInfo = await _classManager.GetClassByIdAsync(classId.Value);
                if (classInfo == null || classInfo.TeacherId != approverId)
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

            // If staff, check if they are the teacher of the student's class
            if (User.IsInRole("Staff"))
            {
                var classId = leave.ClassId;
                if (!classId.HasValue)
                    return BadRequest(new { message = "Leave is not associated with a class" });

                var classInfo = await _classManager.GetClassByIdAsync(classId.Value);
                if (classInfo == null || classInfo.TeacherId != approverId)
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
