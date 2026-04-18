using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Admin
{
    public interface IDepartmentManager : IGenericManager<Department>
    {
        Task<IEnumerable<Department>> GetActiveDepartmentsAsync();
        Task<Department?> GetByCodeAsync(string code);
        Task<IEnumerable<Department>> SearchByNameAsync(string name);
        Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
        Task<IEnumerable<Designation>> GetDepartmentDesignationsAsync(Guid departmentId);
        Task<IEnumerable<Models.Staff>> GetDepartmentStaffAsync(Guid departmentId);
        Task<IEnumerable<Course>> GetDepartmentCoursesAsync(Guid departmentId);
    }
}
