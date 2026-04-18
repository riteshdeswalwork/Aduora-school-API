using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class VisitorEntryManager : GenericManager<VisitorEntry>, IVisitorEntryManager
    {
        public VisitorEntryManager(ApplicationDbContext context) : base(context) { }
    }
}

