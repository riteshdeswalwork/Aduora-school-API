using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class AssetMaintenanceManager : GenericManager<AssetMaintenanceRecord>, IAssetMaintenanceManager
    {
        public AssetMaintenanceManager(ApplicationDbContext context) : base(context) { }
    }
}

