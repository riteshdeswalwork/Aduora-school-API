using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface IDisciplineIncidentManager : IGenericManager<DisciplineIncident>
    {
        Task<PagedResult<DisciplineIncident>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, string? searchTerm, string? status, bool includeInactive);
    }
}
