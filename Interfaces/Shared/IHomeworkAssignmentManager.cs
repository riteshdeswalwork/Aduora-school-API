using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IHomeworkAssignmentManager : IGenericManager<HomeworkAssignment>
    {
        Task<PagedResult<HomeworkAssignment>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, string? searchTerm,
            string? status, string? tab, bool includeInactive, IEnumerable<Guid>? allowedClassIds);
    }
}

