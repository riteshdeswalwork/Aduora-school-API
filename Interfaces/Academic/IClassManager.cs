using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Academic
{
    public interface IClassManager
    {
        Task<IEnumerable<ClassDTO>> GetAllClassesAsync(Guid? academicYearId = null);
        Task<IEnumerable<ClassDTO>> GetClassesByStaffIdAsync(Guid staffId);
        Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(Guid teacherId);
        Task<ClassDTO> GetClassByIdAsync(Guid id);
        Task<ClassDTO> CreateClassAsync(CreateClassDTO classDto);
        Task<ClassDTO> UpdateClassAsync(Guid id, UpdateClassDTO classDto);
        Task<bool> DeleteClassAsync(Guid id);
    }
}
