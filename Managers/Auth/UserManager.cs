using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using sms.api.Constants;
using sms.api.Data;
using sms.api.Interfaces.Auth;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Managers.Auth
{
    public class UserManager : GenericManager<User>, IUserManager
    {
        private readonly IConfiguration _configuration;

        public UserManager(ApplicationDbContext context, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
        }

        public async Task<string> AuthenticateAsync(string username, string password)
        {
            // Find the user by username
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Username == username);

            // Verify password
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return GenerateJwtToken(user);
            }

            return null;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .Include(u => u.Staff)
                .Include(u => u.Student)
                .Include(u => u.Admin)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Staff)
                .Include(u => u.Student)
                .Include(u => u.Admin)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _dbSet
                .Include(u => u.Staff)
                .Include(u => u.Student)
                .Include(u => u.Admin)
                .Where(u => u.Role == role)
                .ToListAsync();
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Username == username);

            // If updating an existing user, exclude their current record
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<User> CreateUserWithRoleAsync(User user, string password, string role)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.Role = role;
            user.CreatedAt = DateTime.UtcNow;

            return await CreateAsync(user);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user != null && BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<int> HashPlainTextPasswordsAsync()
        {
            var usersWithPlainPasswords = await _dbSet
                .Where(u => !string.IsNullOrEmpty(u.PasswordHash) && !u.PasswordHash.StartsWith("$2a$"))
                .ToListAsync();

            int updatedCount = 0;
            foreach (var user in usersWithPlainPasswords)
            {
                // Hash the plain text password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                user.UpdatedAt = DateTime.UtcNow;
                updatedCount++;
            }

            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return updatedCount;
        }

        public async Task<int> CleanupDuplicateUsersAsync()
        {
            // Find duplicate users by username
            var duplicateGroups = await _dbSet
                .GroupBy(u => u.Username)
                .Where(g => g.Count() > 1)
                .ToListAsync();

            int cleanedCount = 0;
            foreach (var group in duplicateGroups)
            {
                var users = group.OrderBy(u => u.CreatedAt).ToList();
                // Keep the first user, remove the rest
                for (int i = 1; i < users.Count; i++)
                {
                    var userToRemove = users[i];

                    // Remove related staff/admin/student records first
                    var staff = await _context.Staff.FirstOrDefaultAsync(s => s.UserId == userToRemove.Id);
                    if (staff != null)
                        _context.Staff.Remove(staff);

                    var admin = await _context.Admins.FirstOrDefaultAsync(a => a.UserId == userToRemove.Id);
                    if (admin != null)
                        _context.Admins.Remove(admin);

                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userToRemove.Id);
                    if (student != null)
                        _context.Students.Remove(student);

                    // Remove the user
                    _context.Users.Remove(userToRemove);
                    cleanedCount++;
                }
            }

            if (cleanedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return cleanedCount;
        }

        public async Task<IReadOnlyList<string>> GetUserPermissionCodesAsync(Guid userId)
        {
            // Try permissions from UserRole -> Role -> RolePermissions -> Permission.Code
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (roleIds.Count > 0)
            {
                var codes = await _context.RolePermissions
                    .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsActive)
                    .Join(_context.Permissions.Where(p => p.IsActive), rp => rp.PermissionId, p => p.Id, (rp, p) => p.Code)
                    .Distinct()
                    .ToListAsync();
                if (codes.Count > 0)
                    return codes;
            }

            // Fallback: role-based defaults when no UserRole/Permission data
            var user = await _dbSet.FindAsync(userId);
            if (user == null) return Array.Empty<string>();

            // Fallback: expand to actions/tabs for Admin/Staff if role-based only
            if (user.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                var codes = new List<string>(PagePermissions.AllPages);
                foreach (var page in PagePermissions.AllPages)
                {
                    foreach (var act in PageActionPermissions.GetActionsForPage(page))
                    {
                        var code = PageActionPermissions.GetActionCode(page, act);
                        if (!string.IsNullOrEmpty(code)) codes.Add(code);
                    }
                    foreach (var tab in PageTabPermissions.GetTabsForPage(page))
                    {
                        var tabCode = PageTabPermissions.GetTabCode(page, tab);
                        if (!string.IsNullOrEmpty(tabCode)) codes.Add(tabCode);
                        foreach (var ta in PageTabPermissions.GetActionsForTab(page, tab))
                        {
                            var tac = PageTabPermissions.GetTabActionCode(page, tab, ta);
                            if (!string.IsNullOrEmpty(tac)) codes.Add(tac);
                        }
                    }
                }
                return codes.Distinct().ToArray();
            }
            if (user.Role?.Equals("Staff", StringComparison.OrdinalIgnoreCase) == true)
            {
                var pages = PagePermissions.StaffDefaultPages;
                var codes = new List<string>(pages);
                foreach (var page in pages)
                {
                    foreach (var act in PageActionPermissions.GetActionsForPage(page))
                    {
                        var code = PageActionPermissions.GetActionCode(page, act);
                        if (!string.IsNullOrEmpty(code)) codes.Add(code);
                    }
                    foreach (var tab in PageTabPermissions.GetTabsForPage(page))
                    {
                        var tabCode = PageTabPermissions.GetTabCode(page, tab);
                        if (!string.IsNullOrEmpty(tabCode)) codes.Add(tabCode);
                        foreach (var ta in PageTabPermissions.GetActionsForTab(page, tab))
                        {
                            var tac = PageTabPermissions.GetTabActionCode(page, tab, ta);
                            if (!string.IsNullOrEmpty(tac)) codes.Add(tac);
                        }
                    }
                }
                return codes.Distinct().ToArray();
            }
            return new[] { PagePermissions.Dashboard, PagePermissions.Settings };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
