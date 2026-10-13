using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface IStudentBehaviorIncidentManager : IGenericManager<StudentBehaviorIncident>
    {
        Task<PagedResult<StudentBehaviorIncident>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, Guid? studentId, Guid? classId,
            DateTime? fromDate, DateTime? toDate, int? severityIndex, string? searchTerm,
            string? status, bool includeInactive);
    }
}
