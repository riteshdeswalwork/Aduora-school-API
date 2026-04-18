using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class InviteeManager : GenericManager<Invitee>, IInviteeManager
    {
        public InviteeManager(ApplicationDbContext context) : base(context) { }
    }
}

