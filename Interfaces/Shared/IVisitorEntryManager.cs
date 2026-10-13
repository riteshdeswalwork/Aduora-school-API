using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IVisitorEntryManager : IGenericManager<VisitorEntry>
    {
        Task<PagedResult<VisitorEntry>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, bool includeInactive,
            bool activeOnly, DateTime? fromDate, DateTime? toDate);
    }
}

