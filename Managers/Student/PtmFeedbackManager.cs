using sms.api.Data;
using sms.api.Interfaces.Student;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Student
{
    public class PtmFeedbackManager : GenericManager<PtmFeedback>, IPtmFeedbackManager
    {
        public PtmFeedbackManager(ApplicationDbContext context) : base(context) { }
    }
}

