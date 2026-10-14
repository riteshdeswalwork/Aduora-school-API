using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface ICoCurricularActivityManager : IGenericManager<CoCurricularActivity>
    {
        Task<PagedResult<CoCurricularActivity>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, string? searchTerm, bool includeInactive);
    }
}
