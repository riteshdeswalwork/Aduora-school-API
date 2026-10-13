using Dapper;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Student;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Services;
using sms.api.Services.Data;

namespace sms.api.Managers.Student
{
    public class DisciplineIncidentManager : GenericManager<DisciplineIncident>, IDisciplineIncidentManager
    {
        private readonly INotificationOrchestrator? _notify;
        private readonly ISqlDataAccess _sqlDataAccess;

        public DisciplineIncidentManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess, INotificationOrchestrator? notify = null) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
            _notify = notify;
        }

        public async Task<PagedResult<DisciplineIncident>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, string? searchTerm, string? status, bool includeInactive)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@AcademicYearId", academicYearId);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@Status", string.IsNullOrWhiteSpace(status) ? null : status.Trim());
            parameters.Add("@IncludeInactive", includeInactive);

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<DisciplineIncident>(
                "dbo.sp_DisciplineIncidents_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<DisciplineIncident>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
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
