using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface IScholarshipManager : IGenericManager<Scholarship>
    {
        Task<PagedResult<Scholarship>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, bool includeInactive);
    }
}
