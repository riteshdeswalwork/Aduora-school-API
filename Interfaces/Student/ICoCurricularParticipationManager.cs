using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface ICoCurricularParticipationManager : IGenericManager<CoCurricularParticipation>
    {
        Task<PagedResult<CoCurricularParticipation>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, Guid? activityId, string? searchTerm, bool includeInactive);
    }
}
