using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Student
{
    public interface IStudentLeaveManager : IGenericManager<StudentLeave>
    {
        Task<IEnumerable<StudentLeave>> GetLeavesByStudentIdAsync(Guid studentId);
        Task<IEnumerable<StudentLeave>> GetLeavesByClassIdAsync(Guid classId);
        Task<IEnumerable<StudentLeave>> GetPendingLeavesForTeacherAsync(Guid teacherId);
        Task<bool> ApproveLeaveAsync(Guid leaveId, Guid approverId);
        Task<bool> RejectLeaveAsync(Guid leaveId, Guid approverId, string reason);
        Task<StudentLeave> CreateStudentLeaveAsync(CreateStudentLeaveDTO leaveDto);
    }
}
