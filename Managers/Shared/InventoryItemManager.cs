using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class InventoryItemManager : GenericManager<InventoryItem>, IInventoryItemManager
    {
        public InventoryItemManager(ApplicationDbContext context) : base(context) { }
    }
}

