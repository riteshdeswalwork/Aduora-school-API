using sms.api.Data;
using sms.api.Interfaces.Student;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Student
{
    public class AchievementTalentManager : GenericManager<AchievementTalent>, IAchievementTalentManager
    {
        public AchievementTalentManager(ApplicationDbContext context) : base(context) { }
    }
}

