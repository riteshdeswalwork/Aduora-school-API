using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Shared
{
    public interface IAnalyticsManager
    {
        Task<DashboardAnalyticsDTO> GetDashboardAnalyticsAsync();
        Task<StudentAnalyticsDTO> GetStudentAnalyticsAsync();
        Task<StaffAnalyticsDTO> GetStaffAnalyticsAsync();
        Task<AttendanceAnalyticsDTO> GetAttendanceAnalyticsAsync();
        Task<FeeAnalyticsDTO> GetFeeAnalyticsAsync();
        Task<ExamAnalyticsDTO> GetExamAnalyticsAsync();
        Task<ClassAnalyticsDTO> GetClassAnalyticsAsync();
        Task<DepartmentAnalyticsDTO> GetDepartmentAnalyticsAsync();
    }
}
