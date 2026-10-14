using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface IHostelRoomManager : IGenericManager<HostelRoom>
    {
        Task<PagedResult<HostelRoom>> GetPagedAsync(
            int pageNumber, int pageSize, Guid? hostelId, string? searchTerm, string? status, bool includeInactive);
    }
}
