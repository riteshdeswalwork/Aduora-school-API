using sms.api.DTOs;

namespace sms.api.Interfaces.Shared
{
    /// <summary>
    /// Cross-entity read for the Admin "all leaves" list: student + staff leaves
    /// combined into one paged, CreatedAt-DESC list (dbo.sp_Leaves_GetAllPaged).
    /// </summary>
    public interface ICombinedLeaveManager
    {
        Task<PagedResult<object>> GetAllLeavesPagedAsync(int pageNumber, int pageSize, string? status = null);
    }
}
