using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Staff;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using StaffEntity = sms.api.Models.Staff;
using StudentEntity = sms.api.Models.Student;

namespace sms.api.Managers.Staff
{
    public class StaffManager : GenericManager<StaffEntity>, IStaffManager
    {
        private readonly IUserManager _userManager;

        public StaffManager(ApplicationDbContext context, IUserManager userManager) : base(context)
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

        public override async Task<IEnumerable<StaffEntity>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(s => s.Courses)
                .Include(s => s.User)
                .Include(s => s.Designation)
                .ToListAsync();
        }

        public override async Task<StaffEntity> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(s => s.Courses)
                .Include(s => s.User)
                .Include(s => s.Designation)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<StaffEntity>> GetStaffByDepartmentAsync(Guid departmentId)
        {
            return await _dbSet
                .Include(s => s.User)
                .Include(s => s.Designation)
                .Where(s => s.Designation != null && s.Designation.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByStaffIdAsync(Guid staffId)
        {
            return await _context.Courses.Where(c => c.StaffId == staffId).ToListAsync();
        }

        public async Task<(IEnumerable<StaffEntity> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Designation)
                    .ThenInclude(d => d.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(s =>
                    (s.User != null && (
                        (s.User.FirstName != null && s.User.FirstName.ToLower().Contains(term)) ||
                        (s.User.LastName != null && s.User.LastName.ToLower().Contains(term)) ||
                        (s.User.FirstName != null && s.User.LastName != null && (s.User.FirstName + " " + s.User.LastName).ToLower().Contains(term)) ||
                        (s.User.Email != null && s.User.Email.ToLower().Contains(term)) ||
                        (s.User.Username != null && s.User.Username.ToLower().Contains(term)))) ||
                    (s.EmployeeId != null && s.EmployeeId.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.User != null ? s.User.FirstName : "")
                .ThenBy(s => s.User != null ? s.User.LastName : "")
                .Skip((pageNumber - 1) * pageSize)
                .Take(Math.Max(1, pageSize))
                .ToListAsync();

            return (items, totalCount);
        }

        public override async Task<StaffEntity> CreateAsync(StaffEntity staff)
        {
            // Validate that User object is provided
            if (staff.User == null)
            {
                throw new InvalidOperationException("User information is required to create staff.");
            }

            // Check if username is unique
            if (!string.IsNullOrEmpty(staff.User.Username))
            {
                var isUsernameUnique = await _userManager.IsUsernameUniqueAsync(staff.User.Username);
                if (!isUsernameUnique)
                {
                    throw new InvalidOperationException($"Username '{staff.User.Username}' is already taken. Please choose a different username.");
                }
            }

            // Use the existing user object from the controller, but ensure proper setup
            var user = staff.User;
            user.Id = Guid.NewGuid();
            user.PasswordHash = EnsurePasswordIsHashed(user.PasswordHash);
            user.Role = "Staff";
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            if (staff.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(staff.RoleId.Value);
                if (role != null && role.IsActive)
                {
                    user.Role = role.Name;
                }
            }

            // Add and save the user first
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (staff.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(staff.RoleId.Value);
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

            // Set the UserId for the staff
            staff.UserId = user.Id;

            // Create the staff
            return await base.CreateAsync(staff);
        }

        public override async Task<StaffEntity> UpdateAsync(StaffEntity staff)
        {
            // Get the user associated with this staff
            var user = await _context.Users.FindAsync(staff.UserId);
            if (user != null)
            {
                // Check if username is unique (excluding current user)
                if (!string.IsNullOrEmpty(staff.User?.Username) && staff.User.Username != user.Username)
                {
                    var isUsernameUnique = await _userManager.IsUsernameUniqueAsync(staff.User.Username, user.Id);
                    if (!isUsernameUnique)
                    {
                        throw new InvalidOperationException($"Username '{staff.User.Username}' is already taken. Please choose a different username.");
                    }
                }

                // Update user properties
                user.FirstName = staff.User?.FirstName ?? user.FirstName;
                user.LastName = staff.User?.LastName ?? user.LastName;
                user.Email = staff.User?.Email ?? user.Email;
                user.Username = staff.User?.Username ?? user.Username;
                user.PhoneNumber = staff.User?.PhoneNumber ?? user.PhoneNumber;
                user.Address = staff.User?.Address ?? user.Address;
                user.UpdatedAt = DateTime.UtcNow;

                // Update password if provided
                if (!string.IsNullOrEmpty(staff.User?.PasswordHash))
                {
                    user.PasswordHash = EnsurePasswordIsHashed(staff.User.PasswordHash);
                }

                if (staff.RoleId.HasValue)
                {
                    var role = await _context.Roles.FindAsync(staff.RoleId.Value);
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

            // Update the staff
            return await base.UpdateAsync(staff);
        }

        public async Task<object> GetStaffDashboardDataAsync(Guid staffUserId)
        {
            // Get staff record by user ID
            var staff = await _dbSet
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == staffUserId);

            if (staff == null)
                return new { MyStudents = 0, MyClasses = 0, PendingLeaves = 0 };

            // Get staff's assigned classes (where this staff is the teacher)
            var myClasses = await _context.Classes
                .Where(c => c.TeacherId == staff.Id)
                .CountAsync();

            // Get students enrolled in staff's classes
            var classIds = await _context.Classes
                .Where(c => c.TeacherId == staff.Id)
                .Select(c => c.Id)
                .ToListAsync();

            var studentIds = await _context.ClassEnrollments
                .Where(e => classIds.Contains(e.ClassId))
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            // Get pending leaves for this staff member
            var pendingLeaves = await _context.StaffLeaves
                .Where(l => l.StaffId == staff.Id && l.Status == "Pending")
                .CountAsync();

            return new
            {
                MyStudents = studentIds,
                MyClasses = myClasses,
                PendingLeaves = pendingLeaves
            };
        }

        public async Task<IEnumerable<Course>> GetStaffClassesAsync(Guid staffUserId)
        {
            var staff = await _dbSet
                .FirstOrDefaultAsync(s => s.UserId == staffUserId);

            if (staff == null)
                return new List<Course>();

            // Note: This method returns Course but we're using Classes in the system
            // This might need to be updated based on your actual requirements
            return new List<Course>();
        }

        public async Task<IEnumerable<StudentEntity>> GetStaffStudentsAsync(Guid staffUserId)
        {
            var staff = await _dbSet
                .FirstOrDefaultAsync(s => s.UserId == staffUserId);

            if (staff == null)
                return new List<StudentEntity>();

            // Get class IDs taught by this staff
            var classIds = await _context.Classes
                .Where(c => c.TeacherId == staff.Id)
                .Select(c => c.Id)
                .ToListAsync();

            // Get students enrolled in these classes
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Class)
                .Where(s => _context.ClassEnrollments
                    .Where(e => classIds.Contains(e.ClassId))
                    .Select(e => e.StudentId)
                    .Contains(s.Id))
                .ToListAsync();

            return students;
        }

        public async Task<StaffEntity?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Designation)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public override async Task<bool> DeleteAsync(Guid id)
        {
            var staff = await _dbSet
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null)
                return false;

            // Store user reference before deleting staff
            var user = staff.User;

            try
            {
                // Delete staff first
                _dbSet.Remove(staff);
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
                throw new InvalidOperationException($"Cannot delete staff member. There might be related records that prevent deletion: {ex.Message}", ex);
            }
        }
    }
}
