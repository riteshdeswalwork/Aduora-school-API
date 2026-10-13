using Dapper;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services;
using sms.api.Services.Data;

namespace sms.api.Managers.Shared
{
    public class VisitorEntryManager : GenericManager<VisitorEntry>, IVisitorEntryManager
    {
        private readonly INotificationOrchestrator? _notify;
        private readonly ISqlDataAccess _sqlDataAccess;

        public VisitorEntryManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess, INotificationOrchestrator? notify = null) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
            _notify = notify;
        }

        public async Task<PagedResult<VisitorEntry>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, bool includeInactive,
            bool activeOnly, DateTime? fromDate, DateTime? toDate)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@IncludeInactive", includeInactive);
            parameters.Add("@ActiveOnly", activeOnly);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<VisitorEntry>(
                "dbo.sp_VisitorEntries_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<VisitorEntry>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
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
