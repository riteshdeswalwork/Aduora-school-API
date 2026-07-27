using Microsoft.EntityFrameworkCore;
using sms.api.Constants;
using sms.api.Data;
using sms.api.Interfaces.Admin;
using sms.api.Models;
using sms.api.Services;

namespace sms.api.Managers.Admin
{
    public class RoleManager : IRoleManager
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationOrchestrator? _notify;

        public RoleManager(ApplicationDbContext context, INotificationOrchestrator? notify = null)
        {
            _context = context;
            _notify = notify;
        }

        public async Task<IEnumerable<Role>> GetRolesAsync(bool includeInactive = false)
        {
            var query = _context.Roles
                .AsNoTracking()
                .Include(r => r.CreatedBy)
                .Include(r => r.UpdatedBy)
                .Include(r => r.UserRoles.Where(ur => ur.IsActive))
                .Include(r => r.RolePermissions.Where(rp => rp.IsActive))
                    .ThenInclude(rp => rp.Permission)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(r => r.IsActive);

            return await query
                .OrderBy(r => r.IsSystemRole ? 0 : 1)
                .ThenBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(Guid id)
        {
            return await _context.Roles
                .AsNoTracking()
                .Include(r => r.CreatedBy)
                .Include(r => r.UpdatedBy)
                .Include(r => r.UserRoles.Where(ur => ur.IsActive))
                    .ThenInclude(ur => ur.User)
                .Include(r => r.RolePermissions.Where(rp => rp.IsActive))
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role> CreateRoleAsync(Role role, Guid createdByUserId)
        {
            var existing = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == role.Name.ToLower());
            if (existing != null)
                throw new InvalidOperationException("Role name already exists");

            role.Id = Guid.NewGuid();
            role.CreatedAt = DateTime.UtcNow;
            role.CreatedByUserId = createdByUserId;
            role.IsSystemRole = false;
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role?> UpdateRoleAsync(Guid id, Role role, Guid updatedByUserId)
        {
            var existing = await _context.Roles.FindAsync(id);
            if (existing == null) return null;
            if (existing.IsSystemRole)
                throw new InvalidOperationException("System roles cannot be modified");

            var nameConflict = await _context.Roles
                .AnyAsync(r => r.Id != id && r.Name.ToLower() == role.Name.ToLower());
            if (nameConflict)
                throw new InvalidOperationException("Role name already exists");

            existing.Name = role.Name;
            existing.Description = role.Description;
            existing.IsActive = role.IsActive;
            existing.DataScope = role.DataScope;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = updatedByUserId;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteRoleAsync(Guid id, Guid deletedByUserId)
        {
            var role = await _context.Roles
                .Include(r => r.UserRoles.Where(ur => ur.IsActive))
                .FirstOrDefaultAsync(r => r.Id == id);
            if (role == null) return false;
            if (role.IsSystemRole)
                throw new InvalidOperationException("System roles cannot be deleted");
            if (role.UserRoles.Any())
                throw new InvalidOperationException("Cannot delete role that is assigned to users");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId)
        {
            return await _context.RolePermissions
                .AsNoTracking()
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId && rp.IsActive)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<bool> AssignPermissionsToRoleAsync(Guid roleId, List<Guid> permissionIds, Guid assignedByUserId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return false;

            var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            foreach (var permissionId in permissionIds)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    PermissionId = permissionId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = assignedByUserId
                });
            }
            await _context.SaveChangesAsync();

            await NotificationEvents.NotifyUsersAsync(
                _notify,
                Enumerable.Empty<Guid>(),
                "Role permissions updated",
                $"Permissions were updated for role \"{role.Name}\" ({permissionIds.Count} permission(s)).",
                "role.permissions.changed",
                "system",
                NotificationChannels.InApp | NotificationChannels.Push | NotificationChannels.Email,
                roleId,
                "Role",
                includeAdmins: true,
                idempotencyKey: $"role.permissions.changed:{roleId}:{DateTime.UtcNow:yyyyMMddHHmm}");

            return true;
        }
    }
}
