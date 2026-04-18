using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;
using sms.api.Models.DTOs;
using sms.api.Controllers.Student;

namespace sms.api.Controllers.Staff
{
    [Authorize]
    public class StaffLeaveController : ApiControllerBase
    {
        private readonly IStaffLeaveManager _staffLeaveManager;

        public StaffLeaveController(IStaffLeaveManager staffLeaveManager)
        {
            _staffLeaveManager = staffLeaveManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var leaves = await _staffLeaveManager.GetAllAsync();
            var mappedLeaves = leaves.Select(MapToResponseDTO);
            return Ok(mappedLeaves);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var leave = await _staffLeaveManager.GetByIdAsync(id);
            if (leave == null)
                return NotFound();

            // If staff, only allow access to own leaves
            if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim) || leave.StaffId.ToString() != staffIdClaim)
                {
                    // Check if the staff is an approver for this leave
                    var isApprover = await _staffLeaveManager.GetApproversForStaffAsync(leave.StaffId);
                    var approverIds = isApprover.Select(a => a.Id.ToString());

                    if (!approverIds.Contains(staffIdClaim))
                        return Forbid();
                }
            }

            return Ok(MapToResponseDTO(leave));
        }

        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetByStaff(Guid staffId)
        {
            // If staff, only allow access to own leaves
            if (User.IsInRole("Staff"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || staffId.ToString() != userIdClaim)
                {
                    // Check if the staff is an approver for this staff member
                    var isApprover = await _staffLeaveManager.GetApproversForStaffAsync(staffId);
                    var approverIds = isApprover.Select(a => a.Id.ToString());

                    if (!approverIds.Contains(userIdClaim))
                        return Forbid();
                }
            }

            var leaves = await _staffLeaveManager.GetLeavesByStaffIdAsync(staffId);
            var mappedLeaves = leaves.Select(MapToResponseDTO);
            return Ok(mappedLeaves);
        }

        [HttpGet("pending/approver")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPendingLeavesForApprover()
        {
            var approverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var leaves = await _staffLeaveManager.GetPendingLeavesForApproverAsync(approverId);
            var mappedLeaves = leaves.Select(MapToResponseDTO);
            return Ok(mappedLeaves);
        }

        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Create([FromBody] CreateStaffLeaveDTO leaveDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ensure the staff is only creating a leave for themselves
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim) || leaveDto.StaffId.ToString() != staffIdClaim)
                return Forbid();

            var leave = await _staffLeaveManager.CreateStaffLeaveAsync(leaveDto);
            return CreatedAtAction(nameof(GetById), new { id = leave.Id }, MapToResponseDTO(leave));
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var leave = await _staffLeaveManager.GetByIdAsync(id);
            if (leave == null)
                return NotFound();

            var approverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _staffLeaveManager.ApproveLeaveAsync(id, approverId);

            if (!result)
                return BadRequest(new { message = "Failed to approve leave or you are not authorized to approve this leave" });

            return Ok(new { message = "Leave approved successfully" });
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLeaveDTO rejectDto)
        {
            var leave = await _staffLeaveManager.GetByIdAsync(id);
            if (leave == null)
                return NotFound();

            var approverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _staffLeaveManager.RejectLeaveAsync(id, approverId, rejectDto.Reason);

            if (!result)
                return BadRequest(new { message = "Failed to reject leave or you are not authorized to reject this leave" });

            return Ok(new { message = "Leave rejected successfully" });
        }

        [HttpPost("approver")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignApprover([FromBody] CreateStaffLeaveApproverDTO approverDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _staffLeaveManager.AssignApproverAsync(approverDto.StaffId, approverDto.ApproverId);
            if (!result)
                return BadRequest(new { message = "Failed to assign approver" });

            return Ok(new { message = "Approver assigned successfully" });
        }

        [HttpGet("approvers/{staffId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetApprovers(Guid staffId)
        {
            var approvers = await _staffLeaveManager.GetApproversForStaffAsync(staffId);
            var mappedApprovers = approvers.Select(a => new
            {
                a.Id,
                a.Designation,
                FirstName = a.User?.FirstName,
                LastName = a.User?.LastName,
                FullName = a.User?.FullName
            });

            return Ok(mappedApprovers);
        }

        private StaffLeaveResponseDTO MapToResponseDTO(StaffLeave leave)
        {
            return new StaffLeaveResponseDTO
            {
                Id = leave.Id,
                StaffId = leave.StaffId,
                StaffName = leave.Staff?.User?.FullName ?? string.Empty,
                LeaveType = leave.LeaveType,
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
}
