using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class ScheduledServiceManager : GenericManager<ScheduledService>, IScheduledServiceManager
    {
        public ScheduledServiceManager(ApplicationDbContext context) : base(context) { }
    }
}

