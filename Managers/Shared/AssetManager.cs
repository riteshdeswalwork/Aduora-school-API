using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class AssetManager : GenericManager<Asset>, IAssetManager
    {
        public AssetManager(ApplicationDbContext context) : base(context) { }
    }
}

