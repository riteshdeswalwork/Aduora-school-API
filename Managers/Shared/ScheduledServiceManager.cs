using Dapper;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services.Data;

namespace sms.api.Managers.Shared
{
    public class ScheduledServiceManager : GenericManager<ScheduledService>, IScheduledServiceManager
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public ScheduledServiceManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<PagedResult<ScheduledService>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, string? status, string? category, bool includeInactive)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@Status", string.IsNullOrWhiteSpace(status) ? null : status.Trim());
            parameters.Add("@Category", string.IsNullOrWhiteSpace(category) ? null : category.Trim());
            parameters.Add("@IncludeInactive", includeInactive);

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<ScheduledService>(
                "dbo.sp_ScheduledServices_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<ScheduledService>
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
