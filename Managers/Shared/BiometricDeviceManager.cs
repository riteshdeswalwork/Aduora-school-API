using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class BiometricDeviceManager : GenericManager<BiometricDevice>, IBiometricDeviceManager
    {
        public BiometricDeviceManager(ApplicationDbContext context) : base(context) { }
    }
}

