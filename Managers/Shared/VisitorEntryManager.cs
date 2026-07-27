using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services;

namespace sms.api.Managers.Shared
{
    public class VisitorEntryManager : GenericManager<VisitorEntry>, IVisitorEntryManager
    {
        private readonly INotificationOrchestrator? _notify;

        public VisitorEntryManager(ApplicationDbContext context, INotificationOrchestrator? notify = null) : base(context)
        {
            _notify = notify;
        }

        public override async Task<VisitorEntry> CreateAsync(VisitorEntry entity)
        {
            var created = await base.CreateAsync(entity);

            var recipients = new List<Guid>();
            if (created.HostStaffId.HasValue)
            {
                var hostUserId = await NotificationEvents.GetStaffUserIdAsync(_context, created.HostStaffId.Value);
                if (hostUserId.HasValue) recipients.Add(hostUserId.Value);
            }

            var admins = await NotificationEvents.GetAdminUserIdsAsync(_context);
            recipients.AddRange(admins);

            var meet = created.PersonToMeet ?? "you";
            await NotificationEvents.NotifyUsersAsync(
                _notify,
                recipients.Distinct(),
                "Visitor check-in",
                $"{created.VisitorName} checked in to meet {meet}. Purpose: {created.Purpose ?? "visit"}.",
                "visitor.created",
                "general",
                NotificationChannels.InApp | NotificationChannels.Push,
                created.Id,
                "VisitorEntry");

            return created;
        }
    }
}
