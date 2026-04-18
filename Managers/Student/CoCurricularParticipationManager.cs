using sms.api.Data;
using sms.api.Interfaces.Student;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Student
{
    public class CoCurricularParticipationManager : GenericManager<CoCurricularParticipation>, ICoCurricularParticipationManager
    {
        public CoCurricularParticipationManager(ApplicationDbContext context) : base(context) { }
    }
}

