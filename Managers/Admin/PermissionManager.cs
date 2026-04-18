using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Admin;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Managers.Admin
{
    public class PermissionManager : IPermissionManager
    {
        private readonly ApplicationDbContext _context;

        public PermissionManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PermissionTreeDto>> GetPermissionTreeAsync(bool includeInactive = false)
        {
            var query = _context.Permissions.AsNoTracking().AsQueryable();
            if (!includeInactive)
                query = query.Where(p => p.IsActive);

            var list = await query
                .OrderBy(p => p.ParentPermissionId == null ? 0 : 1)
                .ThenBy(p => p.Code)
                .Select(p => new PermissionTreeDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    Module = p.Module,
                    Action = p.Action,
                    IsActive = p.IsActive,
                    ParentPermissionId = p.ParentPermissionId
                })
                .ToListAsync();

            var roots = list.Where(p => p.ParentPermissionId == null).ToList();
            var childrenMap = list.Where(p => p.ParentPermissionId != null).GroupBy(p => p.ParentPermissionId!.Value).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var root in roots)
                root.Children = childrenMap.TryGetValue(root.Id, out var children) ? children.OrderBy(c => c.Code).ToList() : new List<PermissionTreeDto>();
            return roots.OrderBy(r => r.Code).ToList();
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync(bool includeInactive = false, string? module = null)
        {
            var query = _context.Permissions
                .AsNoTracking()
                .Include(p => p.CreatedBy)
                .Include(p => p.UpdatedBy)
                .Include(p => p.RolePermissions.Where(rp => rp.IsActive))
                    .ThenInclude(rp => rp.Role)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(p => p.IsActive);
            if (!string.IsNullOrEmpty(module))
                query = query.Where(p => p.Module.ToLower() == module.ToLower());

            return await query
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Action)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetModulesAsync()
        {
            return await _context.Permissions
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Select(p => p.Module)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
        }

        public async Task<Permission?> GetPermissionByIdAsync(Guid id)
        {
            return await _context.Permissions
                .AsNoTracking()
                .Include(p => p.CreatedBy)
                .Include(p => p.UpdatedBy)
                .Include(p => p.RolePermissions.Where(rp => rp.IsActive))
                    .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Permission> CreatePermissionAsync(Permission permission, Guid createdByUserId)
        {
            var existing = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code.ToLower() == permission.Code.ToLower());
            if (existing != null)
                throw new InvalidOperationException("Permission code already exists");

            permission.Id = Guid.NewGuid();
            permission.CreatedAt = DateTime.UtcNow;
            permission.CreatedByUserId = createdByUserId;
            permission.IsSystemPermission = false;
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task<Permission?> UpdatePermissionAsync(Guid id, Permission permission, Guid updatedByUserId)
        {
            var existing = await _context.Permissions.FindAsync(id);
            if (existing == null) return null;
            if (existing.IsSystemPermission)
                throw new InvalidOperationException("System permissions cannot be modified");

            var codeConflict = await _context.Permissions
                .AnyAsync(p => p.Id != id && p.Code.ToLower() == permission.Code.ToLower());
            if (codeConflict)
                throw new InvalidOperationException("Permission code already exists");

            existing.Name = permission.Name;
            existing.Code = permission.Code;
            existing.Description = permission.Description;
            existing.Module = permission.Module;
            existing.Action = permission.Action;
            existing.IsActive = permission.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = updatedByUserId;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePermissionAsync(Guid id, Guid deletedByUserId)
        {
            var permission = await _context.Permissions
                .Include(p => p.RolePermissions.Where(rp => rp.IsActive))
                .FirstOrDefaultAsync(p => p.Id == id);
            if (permission == null) return false;
            if (permission.IsSystemPermission)
                throw new InvalidOperationException("System permissions cannot be deleted");
            if (permission.RolePermissions.Any())
                throw new InvalidOperationException("Cannot delete permission that is assigned to roles");

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SeedDefaultPermissionsAsync(Guid? userId)
        {
            var defaultPermissions = GetDefaultPermissions(userId);
            foreach (var permission in defaultPermissions)
            {
                var exists = await _context.Permissions.AnyAsync(p => p.Code == permission.Code);
                if (!exists)
                    _context.Permissions.Add(permission);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        private static List<Permission> GetDefaultPermissions(Guid? currentUserId)
        {
            var permissions = new List<Permission>();
            var modules = new[]
            {
                "Users", "Students", "Staff", "Classes", "Subjects", "Attendance",
                "Fees", "Exams", "Reports", "Settings", "Roles", "Permissions"
            };
            var actions = new[] { "Create", "Read", "Update", "Delete" };

            foreach (var module in modules)
            {
                foreach (var action in actions)
                {
                    permissions.Add(new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{action} {module}",
                        Code = $"{module.ToLower()}.{action.ToLower()}",
                        Description = $"Permission to {action.ToLower()} {module.ToLower()}",
                        Module = module,
                        Action = action,
                        IsActive = true,
                        IsSystemPermission = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = currentUserId
                    });
                }
            }
            return permissions;
        }
    }
}
