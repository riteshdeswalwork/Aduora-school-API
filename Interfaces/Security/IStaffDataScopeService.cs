using sms.api.Models.Security;

namespace sms.api.Interfaces.Security
{
    public interface IStaffDataScopeService
    {
        Task<StaffScopeContext> ResolveAsync(Guid userId, Guid? academicYearId = null);
        Task<bool> CanAccessClassAsync(Guid userId, Guid classId, Guid? academicYearId = null);
        Task<bool> CanAccessStudentAsync(Guid userId, Guid studentId, Guid? academicYearId = null);
    }
}
