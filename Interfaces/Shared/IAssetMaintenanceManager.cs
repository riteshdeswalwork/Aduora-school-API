using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IAssetMaintenanceManager : IGenericManager<AssetMaintenanceRecord>
    {
        Task<PagedResult<AssetMaintenanceRecord>> GetPagedAsync(
            int pageNumber, int pageSize, Guid? assetId, string? searchTerm, string? status, bool includeInactive);
    }
}
