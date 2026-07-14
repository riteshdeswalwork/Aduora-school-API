using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Shared
{
    public interface IAnalyticsManager
    {
        Task<DashboardAnalyticsDTO> GetDashboardAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null);
        Task<StudentAnalyticsDTO> GetStudentAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null);
        Task<StaffAnalyticsDTO> GetStaffAnalyticsAsync();
        Task<AttendanceAnalyticsDTO> GetAttendanceAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null);
        Task<FeeAnalyticsDTO> GetFeeAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null);
        Task<ExamAnalyticsDTO> GetExamAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null);
        Task<ClassAnalyticsDTO> GetClassAnalyticsAsync(IEnumerable<Guid>? allowedClassIds = null);
        Task<DepartmentAnalyticsDTO> GetDepartmentAnalyticsAsync();
    }
}
