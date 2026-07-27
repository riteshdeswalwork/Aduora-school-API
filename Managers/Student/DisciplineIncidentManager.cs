using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Student;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Services;

namespace sms.api.Managers.Student
{
    public class DisciplineIncidentManager : GenericManager<DisciplineIncident>, IDisciplineIncidentManager
    {
        private readonly INotificationOrchestrator? _notify;

        public DisciplineIncidentManager(ApplicationDbContext context, INotificationOrchestrator? notify = null) : base(context)
        {
            _notify = notify;
        }

        public override async Task<DisciplineIncident> CreateAsync(DisciplineIncident entity)
        {
            var created = await base.CreateAsync(entity);
            await NotifyCreatedAsync(created);
            return created;
        }

        private async Task NotifyCreatedAsync(DisciplineIncident incident)
        {
            if (!incident.StudentId.HasValue) return;

            var studentUserId = await NotificationEvents.GetStudentUserIdAsync(_context, incident.StudentId.Value);
            var classId = await _context.Students.AsNoTracking()
                .Where(s => s.Id == incident.StudentId.Value)
                .Select(s => s.ClassId)
                .FirstOrDefaultAsync();
            var teachers = await NotificationEvents.GetClassTeacherUserIdsAsync(_context, classId);
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
                "Discipline incident recorded",
                body,
                "discipline.created",
                "general",
                NotificationChannels.All,
                incident.Id,
                "DisciplineIncident",
                "warning");
        }
    }
}
