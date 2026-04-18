using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Academic
{
    public interface IClassAttendanceManager : IGenericManager<ClassAttendance>
    {
        Task<ClassAttendance> GetClassAttendanceByIdWithRecordsAsync(Guid id);
        Task<IEnumerable<ClassAttendance>> GetClassAttendancesByClassIdAsync(Guid classId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<ClassAttendance>> GetClassAttendancesByTeacherIdAsync(Guid teacherId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<ClassAttendanceRecord>> GetAttendanceRecordsByStudentIdAsync(Guid studentId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ClassAttendance> CreateClassAttendanceWithRecordsAsync(CreateClassAttendanceDTO attendanceDto);
        Task<bool> UpdateAttendanceRecordAsync(Guid recordId, string status, string remarks);
        Task<ClassAttendanceRecord?> GetAttendanceRecordWithClassAsync(Guid recordId);
    }
}
