using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class EnquiryManager : GenericManager<Enquiry>, IEnquiryManager
    {
        public EnquiryManager(ApplicationDbContext context) : base(context) { }
    }
}

