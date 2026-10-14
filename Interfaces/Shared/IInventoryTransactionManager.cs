using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IInventoryTransactionManager : IGenericManager<InventoryTransaction>
    {
        Task<PagedResult<InventoryTransaction>> GetPagedAsync(
            int pageNumber, int pageSize, Guid? inventoryItemId, string? transactionType, string? searchTerm, bool includeInactive);
    }
}
