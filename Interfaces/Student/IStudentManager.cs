using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface IStudentManager : IGenericManager<Models.Student>
    {
        Task<(IEnumerable<Models.Student> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, Guid? academicYearId = null, IEnumerable<Guid>? allowedStudentIds = null);
        Task<IEnumerable<Models.Student>> GetStudentsByGradeAsync(string grade);
        Task<IEnumerable<Models.Attendance>> GetAttendanceByStudentIdAsync(Guid studentId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<Models.Student?> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<StudentSubject>> GetStudentSubjectsWithSubjectAsync(Guid studentId, string? academicYear = null);
        Task SetStudentSubjectsAsync(Guid studentId, string academicYear, List<Guid> subjectIds, List<Guid>? optionalSubjectIds = null);
        Task<object?> GetStudentAcademicInfoByYearAsync(Guid studentId, Guid academicYearId);
    }
}
