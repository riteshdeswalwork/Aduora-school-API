using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Fee
{
    public interface IOnlinePaymentTransactionManager : IGenericManager<OnlinePaymentTransaction>
    {
        Task<PagedResult<OnlinePaymentTransaction>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, string? searchTerm,
            string? status, string? settlementStatus, bool includeInactive);
    }
}

