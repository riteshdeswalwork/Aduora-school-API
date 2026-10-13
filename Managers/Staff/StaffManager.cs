using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Staff;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Services.Data;
using StaffEntity = sms.api.Models.Staff;
using StudentEntity = sms.api.Models.Student;

namespace sms.api.Managers.Staff
{
    public class StaffManager : GenericManager<StaffEntity>, IStaffManager
    {
        private readonly IUserManager _userManager;
        private readonly ISqlDataAccess _sqlDataAccess;

        public StaffManager(ApplicationDbContext context, IUserManager userManager, ISqlDataAccess sqlDataAccess) : base(context)
        {
            _userManager = userManager;
            _sqlDataAccess = sqlDataAccess;
        }

        // Flat row shape returned by dbo.sp_Staff_GetPaged - mirrors exactly what
        // StaffController's "paged" endpoint reads off the entity/navigations.
        private sealed class StaffPagedRow
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public Guid? DesignationId { get; set; }
            public string? EmployeeId { get; set; }
            public DateTime DateOfJoining { get; set; }
            public string? Address { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Qualification { get; set; }
            public decimal Salary { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public Guid User_Id { get; set; }
            public string? User_FirstName { get; set; }
            public string? User_LastName { get; set; }
            public string? User_Email { get; set; }
            public string? User_Username { get; set; }
            public string? User_Role { get; set; }
            public bool User_IsActive { get; set; }
            public string? User_ProfilePicture { get; set; }
            public Guid? Designation_Id { get; set; }
            public string? Designation_Name { get; set; }
            public string? Designation_Description { get; set; }
            public string? Designation_DepartmentName { get; set; }
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
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", Math.Max(1, pageSize));
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());

            var (rows, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<StaffPagedRow>(
                "dbo.sp_Staff_GetPaged", parameters);

            // Reconstruct lightweight StaffEntity shells with only the fields the
            // controller's "paged" endpoint actually reads, so this manager's return
            // contract (IStaffManager.GetPagedAsync) is unchanged.
            var items = rows.Select(r => new StaffEntity
            {
                Id = r.Id,
                UserId = r.UserId,
                DesignationId = r.DesignationId,
                EmployeeId = r.EmployeeId ?? string.Empty,
                DateOfJoining = r.DateOfJoining,
                Address = r.Address ?? string.Empty,
                PhoneNumber = r.PhoneNumber ?? string.Empty,
                Qualification = r.Qualification ?? string.Empty,
                Salary = r.Salary,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                User = new sms.api.Models.User
                {
                    Id = r.User_Id,
                    FirstName = r.User_FirstName ?? string.Empty,
                    LastName = r.User_LastName ?? string.Empty,
                    Email = r.User_Email ?? string.Empty,
                    Username = r.User_Username ?? string.Empty,
                    Role = r.User_Role ?? string.Empty,
                    IsActive = r.User_IsActive,
                    ProfilePicture = r.User_ProfilePicture,
                },
                Designation = r.Designation_Id.HasValue ? new sms.api.Models.Designation
                {
                    Id = r.Designation_Id.Value,
                    Name = r.Designation_Name ?? string.Empty,
                    Description = r.Designation_Description,
                    Department = new sms.api.Models.Department { Name = r.Designation_DepartmentName ?? "Not assigned" },
                } : null,
            }).ToList();

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
