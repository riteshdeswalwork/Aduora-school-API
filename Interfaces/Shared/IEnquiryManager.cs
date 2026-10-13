using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IEnquiryManager : IGenericManager<Enquiry>
    {
        Task<PagedResult<Enquiry>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, string? status,
            string? priority, string? source, bool? hasFollowUp, bool includeInactive);
    }
}

