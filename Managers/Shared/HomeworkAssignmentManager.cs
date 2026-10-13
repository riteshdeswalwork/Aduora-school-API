using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services;
using sms.api.Services.Data;

namespace sms.api.Managers.Shared
{
    public class HomeworkAssignmentManager : GenericManager<HomeworkAssignment>, IHomeworkAssignmentManager
    {
        private readonly INotificationOrchestrator? _notify;
        private readonly ISqlDataAccess _sqlDataAccess;

        public HomeworkAssignmentManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess, INotificationOrchestrator? notify = null) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
            _notify = notify;
        }

        public async Task<PagedResult<HomeworkAssignment>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, string? searchTerm,
            string? status, string? tab, bool includeInactive, IEnumerable<Guid>? allowedClassIds)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var classSet = allowedClassIds?.ToList();
            var tvp = new DataTable();
            tvp.Columns.Add("Id", typeof(Guid));
            if (classSet != null)
                foreach (var id in classSet) tvp.Rows.Add(id);

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@AcademicYearId", academicYearId);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@Status", string.IsNullOrWhiteSpace(status) ? null : status.Trim());
            parameters.Add("@Tab", string.IsNullOrWhiteSpace(tab) ? null : tab.Trim());
            parameters.Add("@IncludeInactive", includeInactive);
            parameters.Add("@HasAllowedClassFilter", classSet != null);
            parameters.Add("@AllowedClassIds", tvp.AsTableValuedParameter("dbo.GuidListType"));

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<HomeworkAssignment>(
                "dbo.sp_HomeworkAssignments_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<HomeworkAssignment>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
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
