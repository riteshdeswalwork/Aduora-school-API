using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class GalleryMediaManager : GenericManager<GalleryMedia>, IGalleryMediaManager
    {
        public GalleryMediaManager(ApplicationDbContext context) : base(context) { }
    }
}

