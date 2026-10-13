using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface IAchievementTalentManager : IGenericManager<AchievementTalent>
    {
        Task<PagedResult<AchievementTalent>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, string? searchTerm, string? type, bool includeInactive);
    }
}
