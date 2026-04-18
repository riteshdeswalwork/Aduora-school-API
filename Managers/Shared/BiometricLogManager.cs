using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class BiometricLogManager : GenericManager<BiometricLog>, IBiometricLogManager
    {
        public BiometricLogManager(ApplicationDbContext context) : base(context) { }
    }
}

