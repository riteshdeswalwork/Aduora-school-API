using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IInventoryItemManager : IGenericManager<InventoryItem>
    {
        Task<PagedResult<InventoryItem>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, string? category, bool includeInactive);
    }
}
