using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IScheduledServiceManager : IGenericManager<ScheduledService>
    {
        Task<PagedResult<ScheduledService>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, string? status, string? category, bool includeInactive);
    }
}
