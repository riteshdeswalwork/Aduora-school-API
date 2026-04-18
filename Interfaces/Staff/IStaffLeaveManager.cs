using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Staff
{
    public interface IStaffLeaveManager : IGenericManager<StaffLeave>
    {
        Task<IEnumerable<StaffLeave>> GetLeavesByStaffIdAsync(Guid staffId);
        Task<IEnumerable<StaffLeave>> GetPendingLeavesForApproverAsync(Guid approverId);
        Task<bool> ApproveLeaveAsync(Guid leaveId, Guid approverId);
        Task<bool> RejectLeaveAsync(Guid leaveId, Guid approverId, string reason);
        Task<StaffLeave> CreateStaffLeaveAsync(CreateStaffLeaveDTO leaveDto);
        Task<bool> AssignApproverAsync(Guid staffId, Guid approverId);
        Task<IEnumerable<Models.Staff>> GetApproversForStaffAsync(Guid staffId);
    }
}
