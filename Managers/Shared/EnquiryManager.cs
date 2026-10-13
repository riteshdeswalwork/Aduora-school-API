using Dapper;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services;
using sms.api.Services.Data;

namespace sms.api.Managers.Shared
{
    public class EnquiryManager : GenericManager<Enquiry>, IEnquiryManager
    {
        private readonly INotificationOrchestrator? _notify;
        private readonly ISqlDataAccess _sqlDataAccess;

        public EnquiryManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess, INotificationOrchestrator? notify = null) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
            _notify = notify;
        }

        public async Task<PagedResult<Enquiry>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, string? status,
            string? priority, string? source, bool? hasFollowUp, bool includeInactive)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@Status", string.IsNullOrWhiteSpace(status) ? null : status.Trim());
            parameters.Add("@Priority", string.IsNullOrWhiteSpace(priority) ? null : priority.Trim());
            parameters.Add("@Source", string.IsNullOrWhiteSpace(source) ? null : source.Trim());
            parameters.Add("@HasFollowUp", hasFollowUp);
            parameters.Add("@IncludeInactive", includeInactive);

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<Enquiry>(
                "dbo.sp_Enquiries_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<Enquiry>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
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
