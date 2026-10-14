using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IBiometricLogManager : IGenericManager<BiometricLog>
    {
        Task<PagedResult<BiometricLog>> GetPagedAsync(
            int pageNumber, int pageSize, Guid? deviceId, Guid? userId, Guid? studentId, Guid? staffId,
            string? logType, DateTime? from, DateTime? to, bool includeInactive);
    }
}
