using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface IHostelAllocationManager : IGenericManager<HostelAllocation>
    {
        Task<PagedResult<HostelAllocation>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, Guid? hostelRoomId, string? status, bool includeInactive);
    }
}
