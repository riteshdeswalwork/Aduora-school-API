using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services;

namespace sms.api.Managers.Shared
{
    public class WorkAllocationManager : GenericManager<WorkAllocation>, IWorkAllocationManager
    {
        private readonly INotificationOrchestrator? _notify;

        public WorkAllocationManager(ApplicationDbContext context, INotificationOrchestrator? notify = null) : base(context)
        {
            _notify = notify;
        }

        public override async Task<WorkAllocation> CreateAsync(WorkAllocation entity)
        {
            var created = await base.CreateAsync(entity);
            await NotifyAssignedAsync(created);
            return created;
        }

        public override async Task<WorkAllocation> UpdateAsync(WorkAllocation entity)
        {
            var existing = await _context.WorkAllocations.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == entity.Id);
            var wasCompleted = existing != null && IsCompleted(existing.Status);
            var updated = await base.UpdateAsync(entity);
            if (!wasCompleted && IsCompleted(updated.Status))
                await NotifyCompletedAsync(updated, existing);
            else if (existing?.AssignedToStaffId != updated.AssignedToStaffId && updated.AssignedToStaffId.HasValue)
                await NotifyAssignedAsync(updated);
            return updated;
        }

        public async Task<WorkAllocation?> CompleteAsync(Guid id)
        {
            var existing = await _context.WorkAllocations.FindAsync(id);
            if (existing == null || !existing.IsActive) return existing;

            var priorStatus = existing.Status;
            existing.Status = "Completed";
            existing.CompletedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (!IsCompleted(priorStatus))
                await NotifyCompletedAsync(existing, existing);

            return existing;
        }

        private static bool IsCompleted(string? status) =>
            string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase);

        private async Task NotifyAssignedAsync(WorkAllocation allocation)
        {
            if (!allocation.AssignedToStaffId.HasValue) return;

            var assigneeUserId = await NotificationEvents.GetStaffUserIdAsync(_context, allocation.AssignedToStaffId.Value);
            if (!assigneeUserId.HasValue) return;

            var due = allocation.DueDate.HasValue ? allocation.DueDate.Value.ToString("dd MMM") : "not set";
            await NotificationEvents.NotifyUsersAsync(
                _notify,
                new[] { assigneeUserId.Value },
                "Work assigned to you",
                $"{allocation.Title}. Due: {due}. Priority: {allocation.Priority}.",
                "work.assigned",
                "general",
                NotificationChannels.InApp | NotificationChannels.Push | NotificationChannels.Email,
                allocation.Id,
                "WorkAllocation",
                idempotencyKey: $"work.assigned:{allocation.Id}:{allocation.AssignedToStaffId}");
        }

        private async Task NotifyCompletedAsync(WorkAllocation allocation, WorkAllocation? prior)
        {
            var recipients = new List<Guid>();
            if (allocation.AssignedByUserId.HasValue)
                recipients.Add(allocation.AssignedByUserId.Value);
            if (prior?.AssignedByUserId.HasValue == true)
                recipients.Add(prior.AssignedByUserId.Value);

            await NotificationEvents.NotifyUsersAsync(
                _notify,
                recipients.Distinct(),
                "Work allocation completed",
                $"{allocation.Title} has been marked completed.",
                "work.completed",
                "general",
                NotificationChannels.InApp | NotificationChannels.Push,
                allocation.Id,
                "WorkAllocation",
                "success",
                idempotencyKey: $"work.completed:{allocation.Id}");
        }
    }
}
