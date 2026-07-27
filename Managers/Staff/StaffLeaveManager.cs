using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Staff;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;
using sms.api.Services;

namespace sms.api.Managers.Staff
{
    public class StaffLeaveManager : GenericManager<StaffLeave>, IStaffLeaveManager
    {
        private readonly INotificationOrchestrator? _notify;

        public StaffLeaveManager(ApplicationDbContext context, INotificationOrchestrator? notify = null) : base(context)
        {
            _notify = notify;
        }

        public override async Task<IEnumerable<StaffLeave>> GetAllAsync()
        {
            return await _dbSet
                .Include(sl => sl.Staff)
                    .ThenInclude(s => s.User)
                .Include(sl => sl.ApprovedBy)
                .ToListAsync();
        }

        public override async Task<StaffLeave> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(sl => sl.Staff)
                    .ThenInclude(s => s.User)
                .Include(sl => sl.ApprovedBy)
                .FirstOrDefaultAsync(sl => sl.Id == id);
        }

        public async Task<IEnumerable<StaffLeave>> GetLeavesByStaffIdAsync(Guid staffId)
        {
            return await _dbSet
                .Include(sl => sl.Staff)
                    .ThenInclude(s => s.User)
                .Include(sl => sl.ApprovedBy)
                .Where(sl => sl.StaffId == staffId)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<StaffLeave>> GetPendingLeavesForApproverAsync(Guid approverId)
        {
            // Get all staff members that this approver can approve
            var staffIds = await _context.StaffLeaveApprovers
                .Where(sla => sla.ApproverId == approverId && sla.IsActive)
                .Select(sla => sla.StaffId)
                .ToListAsync();

            // Get all pending leaves for these staff members
            return await _dbSet
                .Include(sl => sl.Staff)
                    .ThenInclude(s => s.User)
                .Where(sl => staffIds.Contains(sl.StaffId) && sl.Status == "Pending")
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ApproveLeaveAsync(Guid leaveId, Guid approverId)
        {
            var leave = await GetByIdAsync(leaveId);
            if (leave == null)
                return false;

            // Check if the approver is authorized to approve this leave
            var canApprove = await _context.StaffLeaveApprovers
                .AnyAsync(sla => sla.StaffId == leave.StaffId && sla.ApproverId == approverId && sla.IsActive);

            // Admin can approve any leave
            var isAdmin = await _context.Users
                .AnyAsync(u => u.Id == approverId && u.Role == "Admin");

            if (!canApprove && !isAdmin)
                return false;

            leave.Status = "Approved";
            leave.ApprovedById = approverId;
            leave.ApprovedAt = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var staffUserId = leave.Staff?.UserId ?? await NotificationEvents.GetStaffUserIdAsync(_context, leave.StaffId);
            if (staffUserId.HasValue)
            {
                await NotificationEvents.NotifyUsersAsync(
                    _notify,
                    new[] { staffUserId.Value },
                    "Leave approved",
                    $"Your leave from {leave.StartDate:dd MMM} to {leave.EndDate:dd MMM} was approved.",
                    "leave.approved",
                    "approvals",
                    NotificationChannels.All,
                    leave.Id,
                    "StaffLeave",
                    "success",
                    idempotencyKey: $"staffleave.approved:{leave.Id}");
            }
            return true;
        }

        public async Task<bool> RejectLeaveAsync(Guid leaveId, Guid approverId, string reason)
        {
            var leave = await GetByIdAsync(leaveId);
            if (leave == null)
                return false;

            // Check if the approver is authorized to reject this leave
            var canReject = await _context.StaffLeaveApprovers
                .AnyAsync(sla => sla.StaffId == leave.StaffId && sla.ApproverId == approverId && sla.IsActive);

            // Admin can reject any leave
            var isAdmin = await _context.Users
                .AnyAsync(u => u.Id == approverId && u.Role == "Admin");

            if (!canReject && !isAdmin)
                return false;

            leave.Status = "Rejected";
            leave.RejectionReason = reason;
            leave.ApprovedById = approverId;
            leave.ApprovedAt = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var staffUserId = leave.Staff?.UserId ?? await NotificationEvents.GetStaffUserIdAsync(_context, leave.StaffId);
            if (staffUserId.HasValue)
            {
                await NotificationEvents.NotifyUsersAsync(
                    _notify,
                    new[] { staffUserId.Value },
                    "Leave rejected",
                    $"Your leave request was rejected. {reason}",
                    "leave.rejected",
                    "approvals",
                    NotificationChannels.All,
                    leave.Id,
                    "StaffLeave",
                    "warning",
                    idempotencyKey: $"staffleave.rejected:{leave.Id}");
            }
            return true;
        }

        public async Task<StaffLeave> CreateStaffLeaveAsync(CreateStaffLeaveDTO leaveDto)
        {
            var leave = new StaffLeave
            {
                Id = Guid.NewGuid(),
                StaffId = leaveDto.StaffId,
                StartDate = leaveDto.StartDate,
                EndDate = leaveDto.EndDate,
                Reason = leaveDto.Reason,
                LeaveType = leaveDto.LeaveType,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var created = await CreateAsync(leave);

            var approvers = await NotificationEvents.GetStaffLeaveApproverUserIdsAsync(_context, leave.StaffId);
            var admins = await NotificationEvents.GetAdminUserIdsAsync(_context);
            await NotificationEvents.NotifyUsersAsync(
                _notify,
                approvers.Concat(admins).Distinct(),
                "New staff leave request",
                $"A staff leave request needs approval ({leave.StartDate:dd MMM} – {leave.EndDate:dd MMM}).",
                "leave.created",
                "approvals",
                NotificationChannels.InApp | NotificationChannels.Push | NotificationChannels.Email,
                leave.Id,
                "StaffLeave");

            return created;
        }

        public async Task<bool> AssignApproverAsync(Guid staffId, Guid approverId)
        {
            // Check if the staff and approver exist
            var staffExists = await _context.Staff.AnyAsync(s => s.Id == staffId);
            var approverExists = await _context.Staff.AnyAsync(s => s.Id == approverId);

            if (!staffExists || !approverExists)
                return false;

            // Check if the assignment already exists
            var existingAssignment = await _context.StaffLeaveApprovers
                .FirstOrDefaultAsync(sla => sla.StaffId == staffId && sla.ApproverId == approverId);

            if (existingAssignment != null)
            {
                // Update the existing assignment
                existingAssignment.IsActive = true;
                existingAssignment.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create a new assignment
                var assignment = new StaffLeaveApprover
                {
                    Id = Guid.NewGuid(),
                    StaffId = staffId,
                    ApproverId = approverId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.StaffLeaveApprovers.AddAsync(assignment);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Models.Staff>> GetApproversForStaffAsync(Guid staffId)
        {
            return await _context.StaffLeaveApprovers
                .Where(sla => sla.StaffId == staffId && sla.IsActive)
                .Include(sla => sla.Approver)
                    .ThenInclude(a => a.User)
                .Select(sla => sla.Approver)
                .ToListAsync();
        }
    }
}
