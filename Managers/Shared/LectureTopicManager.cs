using System.Data;
using Dapper;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services.Data;

namespace sms.api.Managers.Shared
{
    public class LectureTopicManager : GenericManager<LectureTopic>, ILectureTopicManager
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public LectureTopicManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<PagedResult<LectureTopic>> GetPagedAsync(
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

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<LectureTopic>(
                "dbo.sp_LectureTopics_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<LectureTopic>
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
