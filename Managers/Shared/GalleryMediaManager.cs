using Dapper;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services.Data;

namespace sms.api.Managers.Shared
{
    public class GalleryMediaManager : GenericManager<GalleryMedia>, IGalleryMediaManager
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public GalleryMediaManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<PagedResult<GalleryMedia>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, string? mediaType,
            string? albumCategory, bool? isPublic, bool includeInactive)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@MediaType", string.IsNullOrWhiteSpace(mediaType) ? null : mediaType.Trim());
            parameters.Add("@AlbumCategory", string.IsNullOrWhiteSpace(albumCategory) ? null : albumCategory.Trim());
            parameters.Add("@IsPublic", isPublic);
            parameters.Add("@IncludeInactive", includeInactive);

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<GalleryMedia>(
                "dbo.sp_GalleryMedia_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<GalleryMedia>
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
