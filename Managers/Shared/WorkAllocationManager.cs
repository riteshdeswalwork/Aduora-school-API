using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class WorkAllocationManager : GenericManager<WorkAllocation>, IWorkAllocationManager
    {
        public WorkAllocationManager(ApplicationDbContext context) : base(context) { }
    }
}

