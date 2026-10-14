using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IInviteeManager : IGenericManager<Invitee>
    {
        Task<PagedResult<Invitee>> GetPagedAsync(
            int pageNumber, int pageSize, Guid? eventId, string? searchTerm, string? rsvpStatus, bool includeInactive);
    }
}
