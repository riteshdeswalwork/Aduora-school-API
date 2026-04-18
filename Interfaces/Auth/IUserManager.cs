using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Auth
{
    public interface IUserManager : IGenericManager<User>
    {
        Task<string> AuthenticateAsync(string username, string password);
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<bool> ValidateCredentialsAsync(string username, string password);
        Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null);
        Task<User> CreateUserWithRoleAsync(User user, string password, string role);
        Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(Guid userId, string newPassword);
        Task<int> HashPlainTextPasswordsAsync();
        Task<int> CleanupDuplicateUsersAsync();
        /// <summary>Gets permission codes (e.g. page.dashboard) for the user from UserRoles + RolePermissions, or role-based default.</summary>
        Task<IReadOnlyList<string>> GetUserPermissionCodesAsync(Guid userId);
    }
}
