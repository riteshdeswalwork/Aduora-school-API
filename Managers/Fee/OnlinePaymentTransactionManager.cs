using sms.api.Data;
using sms.api.Interfaces.Fee;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Fee
{
    public class OnlinePaymentTransactionManager : GenericManager<OnlinePaymentTransaction>, IOnlinePaymentTransactionManager
    {
        public OnlinePaymentTransactionManager(ApplicationDbContext context) : base(context) { }
    }
}

