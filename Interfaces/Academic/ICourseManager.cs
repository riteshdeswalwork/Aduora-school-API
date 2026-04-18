using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Academic
{
    public interface ICourseManager : IGenericManager<Course>
    {
        Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(Guid departmentId);
        Task<IEnumerable<Models.Student>> GetStudentsByCourseIdAsync(Guid courseId);
        Task<bool> AssignStaffToCourseAsync(Guid courseId, Guid staffId);
    }
}
