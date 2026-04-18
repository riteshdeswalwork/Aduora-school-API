using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Admin
{
    public interface IPermissionManager
    {
        Task<IEnumerable<PermissionTreeDto>> GetPermissionTreeAsync(bool includeInactive = false);
        Task<IEnumerable<Permission>> GetPermissionsAsync(bool includeInactive = false, string? module = null);
        Task<IEnumerable<string>> GetModulesAsync();
        Task<Permission?> GetPermissionByIdAsync(Guid id);
        Task<Permission> CreatePermissionAsync(Permission permission, Guid createdByUserId);
        Task<Permission?> UpdatePermissionAsync(Guid id, Permission permission, Guid updatedByUserId);
        Task<bool> DeletePermissionAsync(Guid id, Guid deletedByUserId);
        Task<bool> SeedDefaultPermissionsAsync(Guid? userId);
    }
}
