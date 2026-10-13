using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IWorkAllocationManager : IGenericManager<WorkAllocation>
    {
        Task<WorkAllocation?> CompleteAsync(Guid id);

        Task<PagedResult<WorkAllocation>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, string? status,
            Guid? assignedToStaffId, Guid? assignedToUserId, bool includeInactive);
    }
}

