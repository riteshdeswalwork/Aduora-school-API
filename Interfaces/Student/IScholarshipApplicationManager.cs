using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface IScholarshipApplicationManager : IGenericManager<ScholarshipApplication>
    {
        Task<PagedResult<ScholarshipApplication>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, Guid? scholarshipId, string? status, bool includeInactive);
    }
}
