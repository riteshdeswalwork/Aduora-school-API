using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class InventoryTransactionManager : GenericManager<InventoryTransaction>, IInventoryTransactionManager
    {
        public InventoryTransactionManager(ApplicationDbContext context) : base(context) { }
    }
}

