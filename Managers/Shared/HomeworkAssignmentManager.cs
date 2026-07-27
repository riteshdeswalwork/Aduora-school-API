using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services;

namespace sms.api.Managers.Shared
{
    public class HomeworkAssignmentManager : GenericManager<HomeworkAssignment>, IHomeworkAssignmentManager
    {
        private readonly INotificationOrchestrator? _notify;

        public HomeworkAssignmentManager(ApplicationDbContext context, INotificationOrchestrator? notify = null) : base(context)
        {
            _notify = notify;
        }

        public override async Task<HomeworkAssignment> CreateAsync(HomeworkAssignment entity)
        {
            var created = await base.CreateAsync(entity);
            if (IsPublished(created.Status))
                await NotifyPublishedAsync(created);
            return created;
        }

        public override async Task<HomeworkAssignment> UpdateAsync(HomeworkAssignment entity)
        {
            var existing = await _context.HomeworkAssignments.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == entity.Id);
            var wasPublished = existing != null && IsPublished(existing.Status);
            var updated = await base.UpdateAsync(entity);
            if (!wasPublished && IsPublished(updated.Status))
                await NotifyPublishedAsync(updated);
            return updated;
        }

        private static bool IsPublished(string? status) =>
            string.Equals(status, "Published", StringComparison.OrdinalIgnoreCase);

        private async Task NotifyPublishedAsync(HomeworkAssignment assignment)
        {
            if (!assignment.ClassId.HasValue) return;

            var studentUserIds = await NotificationEvents.GetActiveStudentUserIdsInClassAsync(_context, assignment.ClassId.Value);
            var due = assignment.DueDate.HasValue ? assignment.DueDate.Value.ToString("dd MMM") : "soon";
            var body = assignment.Description ?? "New homework has been assigned.";
            if (body.Length > 200) body = body[..200];

            await NotificationEvents.NotifyUsersAsync(
                _notify,
                studentUserIds,
                assignment.Title ?? "New homework",
                $"{body} Due: {due}.",
                "homework.published",
                "general",
                NotificationChannels.InApp | NotificationChannels.Push | NotificationChannels.Sms,
                assignment.Id,
                "HomeworkAssignment",
                idempotencyKey: $"homework.published:{assignment.Id}");
        }
    }
}
