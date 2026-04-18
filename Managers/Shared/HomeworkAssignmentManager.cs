using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class HomeworkAssignmentManager : GenericManager<HomeworkAssignment>, IHomeworkAssignmentManager
    {
        public HomeworkAssignmentManager(ApplicationDbContext context) : base(context) { }
    }
}

