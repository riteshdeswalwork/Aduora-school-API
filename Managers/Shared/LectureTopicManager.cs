using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class LectureTopicManager : GenericManager<LectureTopic>, ILectureTopicManager
    {
        public LectureTopicManager(ApplicationDbContext context) : base(context) { }
    }
}

