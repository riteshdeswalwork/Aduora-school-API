using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Academic
{
    public interface IClassEnrollmentManager : IGenericManager<ClassEnrollment>
    {
        Task<IEnumerable<ClassEnrollment>> GetEnrollmentsByClassIdAsync(Guid classId);
        Task<IEnumerable<ClassEnrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId);
        Task<ClassEnrollment> GetEnrollmentByStudentAndClassAsync(Guid studentId, Guid classId);
        Task<ClassEnrollment?> GetEnrollmentByStudentAndAcademicYearAsync(Guid studentId, Guid academicYearId);
        Task<bool> UpdateClassEnrollmentAsync(Guid enrollmentId, bool isActive);
        Task<IEnumerable<(Guid ClassId, Guid StudentId, DateTime? EnrollmentDate)>> GetAllEnrollmentsAsync();
    }
}
