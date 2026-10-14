using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using System.Security.Claims;

namespace sms.api.Controllers.Shared
{
    [Route("api/leaves")]
    [ApiController]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly IStudentLeaveManager _studentLeaveManager;
        private readonly IStaffLeaveManager _staffLeaveManager;
        private readonly IStudentManager _studentManager;
        private readonly IStaffManager _staffManager;
        private readonly ICombinedLeaveManager _combinedLeaveManager;

        public LeaveController(
            IStudentLeaveManager studentLeaveManager,
            IStaffLeaveManager staffLeaveManager,
            IStudentManager studentManager,
            IStaffManager staffManager,
            ICombinedLeaveManager combinedLeaveManager)
        {
            _studentLeaveManager = studentLeaveManager;
            _staffLeaveManager = staffLeaveManager;
            _studentManager = studentManager;
            _staffManager = staffManager;
            _combinedLeaveManager = combinedLeaveManager;
        }

        /// <summary>GET api/leaves/my - current user's leaves (student or staff).</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyLeaves()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var combinedLeaves = new List<object>();

            var student = await _studentManager.GetByUserIdAsync(userId);
            if (student != null)
            {
                var studentLeaves = await _studentLeaveManager.GetLeavesByStudentIdAsync(student.Id);
                foreach (var leave in studentLeaves)
                {
                    combinedLeaves.Add(MapStudentLeave(leave));
                }
            }

            var staff = await _staffManager.GetByUserIdAsync(userId);
            if (staff != null)
            {
                var staffLeaves = await _staffLeaveManager.GetLeavesByStaffIdAsync(staff.Id);
                foreach (var leave in staffLeaves)
                {
                    combinedLeaves.Add(MapStaffLeave(leave));
                }
            }

            var sorted = combinedLeaves.OrderByDescending(l => ((dynamic)l).CreatedAt).ToList();
            return Ok(sorted);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var studentLeaves = await _studentLeaveManager.GetAllAsync();
                var staffLeaves = await _staffLeaveManager.GetAllAsync();

                var combinedLeaves = new List<object>();

                // Map student leaves
                foreach (var leave in studentLeaves)
                {
                    combinedLeaves.Add(MapStudentLeave(leave));
                }

                // Map staff leaves
                foreach (var leave in staffLeaves)
                {
                    combinedLeaves.Add(MapStaffLeave(leave));
                }

                // Sort by CreatedAt descending
                var sortedLeaves = combinedLeaves.OrderByDescending(l => ((dynamic)l).CreatedAt).ToList();

                return Ok(sortedLeaves);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET api/leaves/paged - paged, SP-backed (dbo.sp_Leaves_GetAllPaged) version of GetAll:
        /// student + staff leaves combined, CreatedAt DESC, same per-item shape. Optional status filter.
        /// </summary>
        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
        {
            var result = await _combinedLeaveManager.GetAllLeavesPagedAsync(pageNumber, pageSize, status);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                // Try to find in student leaves first
                var studentLeave = await _studentLeaveManager.GetByIdAsync(id);
                if (studentLeave != null)
                {
                    var mappedStudentLeave = new
                    {
                        Id = studentLeave.Id,
                        StudentId = studentLeave.StudentId,
                        StaffId = (Guid?)null,
                        StartDate = studentLeave.StartDate,
                        EndDate = studentLeave.EndDate,
                        Reason = studentLeave.Reason,
                        Status = studentLeave.Status,
                        ApprovedByStaffId = studentLeave.ApprovedById,
                        ApprovedAt = studentLeave.ApprovedAt,
                        ApprovalRemarks = studentLeave.RejectionReason,
                        CreatedAt = studentLeave.CreatedAt,
                        UpdatedAt = studentLeave.UpdatedAt,
                        Student = studentLeave.Student != null ? new
                        {
                            Id = studentLeave.Student.Id,
                            User = studentLeave.Student.User != null ? new
                            {
                                FirstName = studentLeave.Student.User.FirstName,
                                LastName = studentLeave.Student.User.LastName,
                                FullName = $"{studentLeave.Student.User.FirstName} {studentLeave.Student.User.LastName}"
                            } : null
                        } : null,
                        Staff = (object?)null,
                        ApprovedByStaff = studentLeave.ApprovedBy != null ? new
                        {
                            Id = studentLeave.ApprovedBy.Id,
                            FirstName = studentLeave.ApprovedBy.FirstName,
                            LastName = studentLeave.ApprovedBy.LastName,
                            FullName = $"{studentLeave.ApprovedBy.FirstName} {studentLeave.ApprovedBy.LastName}"
                        } : null,
                        TotalDays = (studentLeave.EndDate - studentLeave.StartDate).Days + 1,
                        ApplicantType = "Student"
                    };
                    return Ok(mappedStudentLeave);
                }

                // Try to find in staff leaves
                var staffLeave = await _staffLeaveManager.GetByIdAsync(id);
                if (staffLeave != null)
                {
                    var mappedStaffLeave = new
                    {
                        Id = staffLeave.Id,
                        StudentId = (Guid?)null,
                        StaffId = staffLeave.StaffId,
                        StartDate = staffLeave.StartDate,
                        EndDate = staffLeave.EndDate,
                        Reason = staffLeave.Reason,
                        Status = staffLeave.Status,
                        ApprovedByStaffId = staffLeave.ApprovedById,
                        ApprovedAt = staffLeave.ApprovedAt,
                        ApprovalRemarks = staffLeave.RejectionReason,
                        CreatedAt = staffLeave.CreatedAt,
                        UpdatedAt = staffLeave.UpdatedAt,
                        Student = (object?)null,
                        Staff = staffLeave.Staff != null ? new
                        {
                            Id = staffLeave.Staff.Id,
                            User = staffLeave.Staff.User != null ? new
                            {
                                FirstName = staffLeave.Staff.User.FirstName,
                                LastName = staffLeave.Staff.User.LastName,
                                FullName = $"{staffLeave.Staff.User.FirstName} {staffLeave.Staff.User.LastName}"
                            } : null
                        } : null,
                        ApprovedByStaff = staffLeave.ApprovedBy != null ? new
                        {
                            Id = staffLeave.ApprovedBy.Id,
                            FirstName = staffLeave.ApprovedBy.FirstName,
                            LastName = staffLeave.ApprovedBy.LastName,
                            FullName = $"{staffLeave.ApprovedBy.FirstName} {staffLeave.ApprovedBy.LastName}"
                        } : null,
                        TotalDays = (staffLeave.EndDate - staffLeave.StartDate).Days + 1,
                        ApplicantType = "Staff"
                    };
                    return Ok(mappedStaffLeave);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        private static object MapStudentLeave(StudentLeave leave)
        {
            return new
            {
                Id = leave.Id,
                StudentId = leave.StudentId,
                StaffId = (Guid?)null,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status,
                ApprovedByStaffId = leave.ApprovedById,
                ApprovedAt = leave.ApprovedAt,
                ApprovalRemarks = leave.RejectionReason,
                CreatedAt = leave.CreatedAt,
                UpdatedAt = leave.UpdatedAt,
                Student = leave.Student != null ? new
                {
                    Id = leave.Student.Id,
                    User = leave.Student.User != null ? new
                    {
                        FirstName = leave.Student.User.FirstName,
                        LastName = leave.Student.User.LastName,
                        FullName = $"{leave.Student.User.FirstName} {leave.Student.User.LastName}"
                    } : null
                } : null,
                Staff = (object?)null,
                ApprovedByStaff = leave.ApprovedBy != null ? new
                {
                    Id = leave.ApprovedBy.Id,
                    FirstName = leave.ApprovedBy.FirstName,
                    LastName = leave.ApprovedBy.LastName,
                    FullName = $"{leave.ApprovedBy.FirstName} {leave.ApprovedBy.LastName}"
                } : null,
                TotalDays = (leave.EndDate - leave.StartDate).Days + 1,
                ApplicantType = "Student"
            };
        }

        private static object MapStaffLeave(StaffLeave leave)
        {
            return new
            {
                Id = leave.Id,
                StudentId = (Guid?)null,
                StaffId = leave.StaffId,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status,
                ApprovedByStaffId = leave.ApprovedById,
                ApprovedAt = leave.ApprovedAt,
                ApprovalRemarks = leave.RejectionReason,
                CreatedAt = leave.CreatedAt,
                UpdatedAt = leave.UpdatedAt,
                Student = (object?)null,
                Staff = leave.Staff != null ? new
                {
                    Id = leave.Staff.Id,
                    User = leave.Staff.User != null ? new
                    {
                        FirstName = leave.Staff.User.FirstName,
                        LastName = leave.Staff.User.LastName,
                        FullName = $"{leave.Staff.User.FirstName} {leave.Staff.User.LastName}"
                    } : null
                } : null,
                ApprovedByStaff = leave.ApprovedBy != null ? new
                {
                    Id = leave.ApprovedBy.Id,
                    FirstName = leave.ApprovedBy.FirstName,
                    LastName = leave.ApprovedBy.LastName,
                    FullName = $"{leave.ApprovedBy.FirstName} {leave.ApprovedBy.LastName}"
                } : null,
                TotalDays = (leave.EndDate - leave.StartDate).Days + 1,
                ApplicantType = "Staff"
            };
        }
    }
}
