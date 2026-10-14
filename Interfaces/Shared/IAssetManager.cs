using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IAssetManager : IGenericManager<Asset>
    {
        Task<PagedResult<Asset>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? status, bool includeInactive);
    }
}
