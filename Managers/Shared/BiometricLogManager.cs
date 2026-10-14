using Dapper;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services.Data;

namespace sms.api.Managers.Shared
{
    public class BiometricLogManager : GenericManager<BiometricLog>, IBiometricLogManager
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public BiometricLogManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<PagedResult<BiometricLog>> GetPagedAsync(
            int pageNumber, int pageSize, Guid? deviceId, Guid? userId, Guid? studentId, Guid? staffId,
            string? logType, DateTime? from, DateTime? to, bool includeInactive)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@DeviceId", deviceId);
            parameters.Add("@UserId", userId);
            parameters.Add("@StudentId", studentId);
            parameters.Add("@StaffId", staffId);
            parameters.Add("@LogType", string.IsNullOrWhiteSpace(logType) ? null : logType.Trim());
            parameters.Add("@FromDate", from);
            parameters.Add("@ToDate", to);
            parameters.Add("@IncludeInactive", includeInactive);

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<BiometricLog>(
                "dbo.sp_BiometricLogs_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<BiometricLog>
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
