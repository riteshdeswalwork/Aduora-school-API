using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services;

namespace sms.api.Managers.Shared
{
    public class EnquiryManager : GenericManager<Enquiry>, IEnquiryManager
    {
        private readonly INotificationOrchestrator? _notify;

        public EnquiryManager(ApplicationDbContext context, INotificationOrchestrator? notify = null) : base(context)
        {
            _notify = notify;
        }

        public override async Task<Enquiry> CreateAsync(Enquiry entity)
        {
            var created = await base.CreateAsync(entity);
            var admins = await NotificationEvents.GetAdminUserIdsAsync(_context);
            await NotificationEvents.NotifyUsersAsync(
                _notify,
                admins,
                "New enquiry received",
                $"{created.Name} — {created.SeekingClass ?? created.CourseInterested ?? "admission enquiry"}. Contact: {created.ContactNumber ?? created.Email ?? "N/A"}.",
                "enquiry.created",
                "general",
                NotificationChannels.InApp | NotificationChannels.Push | NotificationChannels.Email,
                created.Id,
                "Enquiry");
            return created;
        }
    }
}
