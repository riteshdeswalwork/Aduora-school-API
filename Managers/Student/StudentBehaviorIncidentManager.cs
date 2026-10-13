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
    public class StudentBehaviorIncidentManager : GenericManager<StudentBehaviorIncident>, IStudentBehaviorIncidentManager
    {
        private readonly INotificationOrchestrator? _notify;
        private readonly ISqlDataAccess _sqlDataAccess;

        public StudentBehaviorIncidentManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess, INotificationOrchestrator? notify = null) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
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

        // Flat row shape returned by dbo.sp_StudentBehaviorIncidents_GetPaged -
        // mirrors what the controller's ApplyIncludes() (Student.User, Class) needs.
        private sealed class BehaviorIncidentRow
        {
            public Guid Id { get; set; }
            public Guid AcademicYearId { get; set; }
            public Guid? StudentId { get; set; }
            public Guid? ClassId { get; set; }
            public DateTime IncidentDate { get; set; }
            public string? BehaviorType { get; set; }
            public string? Archetype { get; set; }
            public int SeverityIndex { get; set; }
            public string? Description { get; set; }
            public string? InterventionPlan { get; set; }
            public bool PsychReferral { get; set; }
            public string? ParentFeedbackCycle { get; set; }
            public DateTime? FollowUpDate { get; set; }
            public string? Status { get; set; }
            public Guid? ReportedByUserId { get; set; }
            public string? LocationOfIncident { get; set; }
            public string? Witnesses { get; set; }
            public string? ActionTaken { get; set; }
            public string? ConsequenceType { get; set; }
            public DateTime? ResolvedAt { get; set; }
            public string? Notes { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public Guid? Stu_Id { get; set; }
            public string? Stu_RollNumber { get; set; }
            public string? Stu_FirstName { get; set; }
            public string? Stu_LastName { get; set; }
            public Guid? Class_Id { get; set; }
            public string? Class_Name { get; set; }
        }

        public async Task<PagedResult<StudentBehaviorIncident>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, Guid? studentId, Guid? classId,
            DateTime? fromDate, DateTime? toDate, int? severityIndex, string? searchTerm,
            string? status, bool includeInactive)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@AcademicYearId", academicYearId);
            parameters.Add("@StudentId", studentId);
            parameters.Add("@ClassId", classId);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            parameters.Add("@SeverityIndex", severityIndex);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@Status", string.IsNullOrWhiteSpace(status) ? null : status.Trim());
            parameters.Add("@IncludeInactive", includeInactive);

            using var connection = await _sqlDataAccess.CreateOpenConnectionAsync();
            using var multi = await connection.QueryMultipleAsync(
                "dbo.sp_StudentBehaviorIncidents_GetPaged", parameters, commandType: System.Data.CommandType.StoredProcedure);

            var totalCount = await multi.ReadSingleAsync<int>();
            var rows = (await multi.ReadAsync<BehaviorIncidentRow>()).ToList();

            var items = rows.Select(r => new StudentBehaviorIncident
            {
                Id = r.Id,
                AcademicYearId = r.AcademicYearId,
                StudentId = r.StudentId,
                ClassId = r.ClassId,
                IncidentDate = r.IncidentDate,
                BehaviorType = r.BehaviorType,
                Archetype = r.Archetype,
                SeverityIndex = r.SeverityIndex,
                Description = r.Description ?? string.Empty,
                InterventionPlan = r.InterventionPlan,
                PsychReferral = r.PsychReferral,
                ParentFeedbackCycle = r.ParentFeedbackCycle,
                FollowUpDate = r.FollowUpDate,
                Status = r.Status ?? string.Empty,
                ReportedByUserId = r.ReportedByUserId,
                LocationOfIncident = r.LocationOfIncident,
                Witnesses = r.Witnesses,
                ActionTaken = r.ActionTaken,
                ConsequenceType = r.ConsequenceType,
                ResolvedAt = r.ResolvedAt,
                Notes = r.Notes,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Student = r.Stu_Id.HasValue ? new sms.api.Models.Student
                {
                    Id = r.Stu_Id.Value,
                    RollNumber = r.Stu_RollNumber ?? string.Empty,
                    User = new sms.api.Models.User { FirstName = r.Stu_FirstName ?? string.Empty, LastName = r.Stu_LastName ?? string.Empty },
                } : null,
                Class = r.Class_Id.HasValue ? new sms.api.Models.Class { Id = r.Class_Id.Value, Name = r.Class_Name ?? string.Empty } : null,
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<StudentBehaviorIncident>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }
    }
}
