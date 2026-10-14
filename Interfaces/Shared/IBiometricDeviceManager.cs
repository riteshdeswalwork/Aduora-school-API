using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IBiometricDeviceManager : IGenericManager<BiometricDevice>
    {
        Task<PagedResult<BiometricDevice>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? status, bool includeInactive);
    }
}
