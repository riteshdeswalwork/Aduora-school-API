using Microsoft.EntityFrameworkCore;
using sms.api.Constants;
using sms.api.Data;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Security;
using sms.api.Interfaces.Staff;
using sms.api.Models.Security;

namespace sms.api.Managers.Security
{
    public class StaffDataScopeService : IStaffDataScopeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStaffManager _staffManager;
        private readonly IClassManager _classManager;
        private readonly IClassEnrollmentManager _classEnrollmentManager;

        public StaffDataScopeService(
            ApplicationDbContext context,
            IStaffManager staffManager,
            IClassManager classManager,
            IClassEnrollmentManager classEnrollmentManager)
        {
            _context = context;
            _staffManager = staffManager;
            _classManager = classManager;
            _classEnrollmentManager = classEnrollmentManager;
        }

        public async Task<StaffScopeContext> ResolveAsync(Guid userId, Guid? academicYearId = null)
        {
            var effectiveScope = await ResolveEffectiveScopeAsync(userId);

            if (effectiveScope == DataScopeType.Global)
            {
                return new StaffScopeContext
                {
                    EffectiveScope = DataScopeType.Global,
                    AllowedClassIds = null,
                    AllowedStudentIds = null
                };
            }

            if (effectiveScope == DataScopeType.OwnOnly)
            {
                var staff = await _staffManager.GetByUserIdAsync(userId);
                return new StaffScopeContext
                {
                    EffectiveScope = DataScopeType.OwnOnly,
                    StaffId = staff?.Id,
                    AllowedClassIds = Array.Empty<Guid>(),
                    AllowedStudentIds = Array.Empty<Guid>()
                };
            }

            // AssignedClasses
            var staffRecord = await _staffManager.GetByUserIdAsync(userId);
            if (staffRecord == null)
            {
                return new StaffScopeContext
                {
                    EffectiveScope = DataScopeType.AssignedClasses,
                    AllowedClassIds = Array.Empty<Guid>(),
                    AllowedStudentIds = Array.Empty<Guid>()
                };
            }

            var allClasses = await _classManager.GetAllClassesAsync(academicYearId);
            var staffClasses = await _classManager.GetClassesByStaffIdAsync(staffRecord.Id);
            var staffClassIdSet = staffClasses.Select(c => c.Id).ToHashSet();
            var classIds = allClasses.Where(c => staffClassIdSet.Contains(c.Id)).Select(c => c.Id).Distinct().ToList();

            var studentIds = await GetStudentIdsForClassesAsync(classIds);

            return new StaffScopeContext
            {
                EffectiveScope = DataScopeType.AssignedClasses,
                StaffId = staffRecord.Id,
                AllowedClassIds = classIds,
                AllowedStudentIds = studentIds
            };
        }

        public async Task<bool> CanAccessClassAsync(Guid userId, Guid classId, Guid? academicYearId = null)
        {
            var scope = await ResolveAsync(userId, academicYearId);
            return scope.CanAccessClass(classId);
        }

        public async Task<bool> CanAccessStudentAsync(Guid userId, Guid studentId, Guid? academicYearId = null)
        {
            var scope = await ResolveAsync(userId, academicYearId);
            return scope.CanAccessStudent(studentId);
        }

        private async Task<DataScopeType> ResolveEffectiveScopeAsync(Guid userId)
        {
            var roleScopes = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Join(
                    _context.Roles.AsNoTracking().Where(r => r.IsActive),
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.DataScope)
                .ToListAsync();

            if (roleScopes.Count > 0)
                return MostPermissive(roleScopes);

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return DataScopeType.OwnOnly;

            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return DataScopeType.Global;
            if (string.Equals(user.Role, "Staff", StringComparison.OrdinalIgnoreCase))
                return DataScopeType.AssignedClasses;

            return DataScopeType.OwnOnly;
        }

        private static DataScopeType MostPermissive(IEnumerable<DataScopeType> scopes)
        {
            if (scopes.Any(s => s == DataScopeType.Global)) return DataScopeType.Global;
            if (scopes.Any(s => s == DataScopeType.AssignedClasses)) return DataScopeType.AssignedClasses;
            return DataScopeType.OwnOnly;
        }

        private async Task<List<Guid>> GetStudentIdsForClassesAsync(IReadOnlyList<Guid> classIds)
        {
            if (classIds.Count == 0) return new List<Guid>();

            var studentIds = new HashSet<Guid>();
            foreach (var classId in classIds)
            {
                var enrollments = await _classEnrollmentManager.GetEnrollmentsByClassIdAsync(classId);
                foreach (var enrollment in enrollments)
                    studentIds.Add(enrollment.StudentId);
            }
            return studentIds.ToList();
        }
    }
}
