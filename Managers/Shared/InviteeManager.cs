using Dapper;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services.Data;

namespace sms.api.Managers.Shared
{
    public class InviteeManager : GenericManager<Invitee>, IInviteeManager
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public InviteeManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<PagedResult<Invitee>> GetPagedAsync(
            int pageNumber, int pageSize, Guid? eventId, string? searchTerm, string? rsvpStatus, bool includeInactive)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@EventId", eventId);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@RsvpStatus", string.IsNullOrWhiteSpace(rsvpStatus) ? null : rsvpStatus.Trim());
            parameters.Add("@IncludeInactive", includeInactive);

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<Invitee>(
                "dbo.sp_Invitees_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<Invitee>
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
