using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Student;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Services;

namespace sms.api.Managers.Student
{
    public class StudentBehaviorIncidentManager : GenericManager<StudentBehaviorIncident>, IStudentBehaviorIncidentManager
    {
        private readonly INotificationOrchestrator? _notify;

        public StudentBehaviorIncidentManager(ApplicationDbContext context, INotificationOrchestrator? notify = null) : base(context)
        {
            _notify = notify;
        }

        public override async Task<StudentBehaviorIncident> CreateAsync(StudentBehaviorIncident entity)
        {
            var created = await base.CreateAsync(entity);
            await NotifyCreatedAsync(created);
            return created;
        }

        private async Task NotifyCreatedAsync(StudentBehaviorIncident incident)
        {
            if (!incident.StudentId.HasValue) return;

            var studentUserId = await NotificationEvents.GetStudentUserIdAsync(_context, incident.StudentId.Value);
            var teachers = await NotificationEvents.GetClassTeacherUserIdsAsync(_context, incident.ClassId);
            var admins = await NotificationEvents.GetAdminUserIdsAsync(_context);

            var recipients = new List<Guid>();
            if (studentUserId.HasValue) recipients.Add(studentUserId.Value);
            recipients.AddRange(teachers);
            recipients.AddRange(admins);

            var body = incident.Description;
            if (body.Length > 200) body = body[..200];

            await NotificationEvents.NotifyUsersAsync(
                _notify,
                recipients.Distinct(),
                "Behavior incident recorded",
                body,
                "behavior.created",
                "general",
                NotificationChannels.All,
                incident.Id,
                "StudentBehaviorIncident",
                "warning");
        }
    }
}
