using sms.api.Models;

namespace sms.api.Interfaces.Admin
{
    public interface IRoleManager
    {
        Task<IEnumerable<Role>> GetRolesAsync(bool includeInactive = false);
        Task<Role?> GetRoleByIdAsync(Guid id);
        Task<Role> CreateRoleAsync(Role role, Guid createdByUserId);
        Task<Role?> UpdateRoleAsync(Guid id, Role role, Guid updatedByUserId);
        Task<bool> DeleteRoleAsync(Guid id, Guid deletedByUserId);
        Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId);
        Task<bool> AssignPermissionsToRoleAsync(Guid roleId, List<Guid> permissionIds, Guid assignedByUserId);
    }
}
