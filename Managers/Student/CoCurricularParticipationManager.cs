using Dapper;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Student;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Services.Data;

namespace sms.api.Managers.Student
{
    public class CoCurricularParticipationManager : GenericManager<CoCurricularParticipation>, ICoCurricularParticipationManager
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public CoCurricularParticipationManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<PagedResult<CoCurricularParticipation>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, Guid? activityId, string? searchTerm, bool includeInactive)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@AcademicYearId", academicYearId);
            parameters.Add("@ActivityId", activityId);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@IncludeInactive", includeInactive);

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<CoCurricularParticipation>(
                "dbo.sp_CoCurricularParticipations_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<CoCurricularParticipation>
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
