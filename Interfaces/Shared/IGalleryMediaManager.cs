using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IGalleryMediaManager : IGenericManager<GalleryMedia>
    {
        Task<PagedResult<GalleryMedia>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, string? mediaType,
            string? albumCategory, bool? isPublic, bool includeInactive);
    }
}
