using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Admin;
using sms.api.Interfaces.Auth;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using AdminEntity = sms.api.Models.Admin;

namespace sms.api.Managers.Admin
{
    public class AdminManager : GenericManager<AdminEntity>, IAdminManager
    {
        private readonly IUserManager _userManager;

        public AdminManager(ApplicationDbContext context, IUserManager userManager) : base(context)
        {
            _userManager = userManager;
        }

        private string EnsurePasswordIsHashed(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            // Check if password is already hashed (BCrypt hashes start with $2a$, $2b$, or $2y$)
            if (password.StartsWith("$2a$") || password.StartsWith("$2b$") || password.StartsWith("$2y$"))
                return password;

            // Hash the plain text password
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public override async Task<IEnumerable<AdminEntity>> GetAllAsync()
        {
            return await _dbSet
                .Include(a => a.User)
                .ToListAsync();
        }

        public override async Task<AdminEntity> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public override async Task<AdminEntity> CreateAsync(AdminEntity admin)
        {
            // Validate that User object is provided
            if (admin.User == null)
            {
                throw new InvalidOperationException("User information is required to create admin.");
            }

            // Check if username is unique
            if (!string.IsNullOrEmpty(admin.User.Username))
            {
                var isUsernameUnique = await _userManager.IsUsernameUniqueAsync(admin.User.Username);
                if (!isUsernameUnique)
                {
                    throw new InvalidOperationException($"Username '{admin.User.Username}' is already taken. Please choose a different username.");
                }
            }

            // Use the existing user object from the controller, but ensure proper setup
            var user = admin.User;
            user.Id = Guid.NewGuid();
            user.PasswordHash = EnsurePasswordIsHashed(user.PasswordHash);
            user.Role = "Admin";
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            if (admin.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(admin.RoleId.Value);
                if (role != null && role.IsActive)
                {
                    user.Role = role.Name;
                }
            }

            // Add and save the user first
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (admin.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(admin.RoleId.Value);
                if (role != null && role.IsActive)
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RoleId = role.Id,
                        IsActive = true,
                        AssignedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
            }

            // Set the UserId for the admin
            admin.UserId = user.Id;

            // Create the admin
            return await base.CreateAsync(admin);
        }

        public override async Task<AdminEntity> UpdateAsync(AdminEntity admin)
        {
            // Get the user associated with this admin
            var user = await _context.Users.FindAsync(admin.UserId);
            if (user != null)
            {
                // Check if username is unique (excluding current user)
                if (!string.IsNullOrEmpty(admin.User?.Username) && admin.User.Username != user.Username)
                {
                    var isUsernameUnique = await _userManager.IsUsernameUniqueAsync(admin.User.Username, user.Id);
                    if (!isUsernameUnique)
                    {
                        throw new InvalidOperationException($"Username '{admin.User.Username}' is already taken. Please choose a different username.");
                    }
                }

                // Update user properties
                user.FirstName = admin.User?.FirstName ?? user.FirstName;
                user.LastName = admin.User?.LastName ?? user.LastName;
                user.Email = admin.User?.Email ?? user.Email;
                user.Username = admin.User?.Username ?? user.Username;
                user.PhoneNumber = admin.User?.PhoneNumber ?? user.PhoneNumber;
                user.Address = admin.User?.Address ?? user.Address;
                user.UpdatedAt = DateTime.UtcNow;

                // Update password if provided
                if (!string.IsNullOrEmpty(admin.User?.PasswordHash))
                {
                    user.PasswordHash = EnsurePasswordIsHashed(admin.User.PasswordHash);
                }

                if (admin.RoleId.HasValue)
                {
                    var role = await _context.Roles.FindAsync(admin.RoleId.Value);
                    if (role != null && role.IsActive)
                    {
                        user.Role = role.Name;
                        var existingUserRoles = await _context.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync();
                        foreach (var ur in existingUserRoles)
                        {
                            if (ur.RoleId == role.Id)
                            {
                                ur.IsActive = true;
                                ur.RevokedAt = null;
                                ur.AssignedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                ur.IsActive = false;
                                ur.RevokedAt = DateTime.UtcNow;
                            }
                        }
                        var alreadyHasRole = existingUserRoles.Any(ur => ur.RoleId == role.Id);
                        if (!alreadyHasRole)
                        {
                            _context.UserRoles.Add(new UserRole
                            {
                                Id = Guid.NewGuid(),
                                UserId = user.Id,
                                RoleId = role.Id,
                                IsActive = true,
                                AssignedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                // Update the user
                _context.Users.Update(user);
            }

            // Update the admin
            return await base.UpdateAsync(admin);
        }

        public override async Task<bool> DeleteAsync(Guid id)
        {
            var admin = await _dbSet
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (admin == null)
                return false;

            // Store user reference before deleting admin
            var user = admin.User;

            try
            {
                // Delete admin first
                _dbSet.Remove(admin);
                await _context.SaveChangesAsync();

                // Delete associated user if it exists
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the error (you can add logging here)
                // If there are foreign key constraints, we might need to handle them
                throw new InvalidOperationException($"Cannot delete administrator. There might be related records that prevent deletion: {ex.Message}", ex);
            }
        }
    }
}
