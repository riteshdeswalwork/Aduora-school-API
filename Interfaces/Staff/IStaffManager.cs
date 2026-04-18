using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Staff
{
    public interface IStaffManager : IGenericManager<Models.Staff>
    {
        Task<IEnumerable<Models.Staff>> GetStaffByDepartmentAsync(Guid departmentId);
        Task<IEnumerable<Course>> GetCoursesByStaffIdAsync(Guid staffId);
        Task<object> GetStaffDashboardDataAsync(Guid staffUserId);
        Task<IEnumerable<Course>> GetStaffClassesAsync(Guid staffUserId);
        Task<IEnumerable<Models.Student>> GetStaffStudentsAsync(Guid staffUserId);
        Task<Models.Staff?> GetByUserIdAsync(Guid userId);
        Task<(IEnumerable<Models.Staff> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
    }
}
