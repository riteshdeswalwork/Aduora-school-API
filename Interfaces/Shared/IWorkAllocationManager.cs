using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IWorkAllocationManager : IGenericManager<WorkAllocation>
    {
        Task<WorkAllocation?> CompleteAsync(Guid id);
    }
}

