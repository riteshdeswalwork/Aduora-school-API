using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface IHostelManager : IGenericManager<Hostel>
    {
        Task<PagedResult<Hostel>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, bool includeInactive);
    }
}
