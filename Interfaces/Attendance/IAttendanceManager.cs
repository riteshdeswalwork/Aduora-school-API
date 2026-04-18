using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Attendance
{
    public interface IAttendanceManager : IGenericManager<Models.Attendance>
    {
        Task<IEnumerable<Models.Attendance>> GetAttendanceByStudentIdAsync(Guid studentId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<Models.Attendance>> GetAttendanceByCourseIdAsync(Guid courseId, DateTime? date = null);
        Task<bool> MarkAttendanceAsync(Guid studentId, Guid courseId, DateTime date, bool isPresent, string? remarks = null);
    }
}
