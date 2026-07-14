using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Student;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using StudentEntity = sms.api.Models.Student;

namespace sms.api.Managers.Student
{
    public class StudentManager : GenericManager<StudentEntity>, IStudentManager
    {
        private readonly IUserManager _userManager;
        private readonly IAcademicYearManager _academicYearManager;

        public StudentManager(ApplicationDbContext context, IUserManager userManager, IAcademicYearManager academicYearManager) : base(context)
        {
            _userManager = userManager;
            _academicYearManager = academicYearManager;
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

        public override async Task<IEnumerable<StudentEntity>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(s => s.Attendances)
                .Include(s => s.User)
                .Include(s => s.Class)
                .ToListAsync();
        }

        public override async Task<StudentEntity> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(s => s.Attendances)
                .Include(s => s.User)
                .Include(s => s.Class)
                    .ThenInclude(c => c.ClassSubjects)
                        .ThenInclude(cs => cs.Subject)
                            .ThenInclude(sub => sub.Staff)
                                .ThenInclude(staff => staff.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<StudentEntity>> GetStudentsByGradeAsync(string grade)
        {
            return await _dbSet
                .Include(s => s.User)
                .Include(s => s.Class)
                .Where(s => s.CurrentGrade == grade)
                .ToListAsync();
        }

        public async Task<(IEnumerable<StudentEntity> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, Guid? academicYearId = null, IEnumerable<Guid>? allowedStudentIds = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Class)
                .AsQueryable();

            if (academicYearId.HasValue)
            {
                // Ignore global academic-year header filter so query param controls the list.
                var enrolledStudentIds = await _context.ClassEnrollments
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(e => e.AcademicYearId == academicYearId.Value && e.IsActive)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .ToListAsync();

                if (enrolledStudentIds.Count > 0)
                {
                    var studentsWithAnyEnrollment = await _context.ClassEnrollments
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .Where(e => e.IsActive)
                        .Select(e => e.StudentId)
                        .Distinct()
                        .ToListAsync();

                    // Enrolled in this year, or no enrollment row yet (legacy / import without ClassEnrollment).
                    query = query.Where(s =>
                        enrolledStudentIds.Contains(s.Id) ||
                        !studentsWithAnyEnrollment.Contains(s.Id));
                }
            }

            if (allowedStudentIds != null)
            {
                var idList = allowedStudentIds.ToList();
                if (idList.Count == 0)
                    return (new List<StudentEntity>(), 0);
                query = query.Where(s => idList.Contains(s.Id));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(s =>
                    (s.User != null && (
                        (s.User.FirstName != null && s.User.FirstName.ToLower().Contains(term)) ||
                        (s.User.LastName != null && s.User.LastName.ToLower().Contains(term)) ||
                        (s.User.Email != null && s.User.Email.ToLower().Contains(term)) ||
                        (s.User.Username != null && s.User.Username.ToLower().Contains(term)))) ||
                    (s.RegistrationNumber != null && s.RegistrationNumber.ToLower().Contains(term)) ||
                    (s.RollNumber != null && s.RollNumber.ToLower().Contains(term)) ||
                    (s.AdmissionNumber != null && s.AdmissionNumber.ToLower().Contains(term)));
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

        public async Task<IEnumerable<Models.Attendance>> GetAttendanceByStudentIdAsync(Guid studentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Attendances
                .Include(a => a.Course)
                .Where(a => a.StudentId == studentId);

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);

            return await query.OrderByDescending(a => a.Date).ToListAsync();
        }

        public override async Task<StudentEntity> CreateAsync(StudentEntity student)
        {
            // Validate that User object is provided
            if (student.User == null)
            {
                throw new InvalidOperationException("User information is required to create student.");
            }

            // Check if username is unique
            if (!string.IsNullOrEmpty(student.User.Username))
            {
                var isUsernameUnique = await _userManager.IsUsernameUniqueAsync(student.User.Username);
                if (!isUsernameUnique)
                {
                    throw new InvalidOperationException($"Username '{student.User.Username}' is already taken. Please choose a different username.");
                }
            }

            // Use the existing user object from the controller, but ensure proper setup
            var user = student.User;
            user.Id = Guid.NewGuid();
            user.PasswordHash = EnsurePasswordIsHashed(user.PasswordHash);
            user.Role = "Student";
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            if (student.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(student.RoleId.Value);
                if (role != null && role.IsActive)
                {
                    user.Role = role.Name;
                }
            }

            // Add and save the user first
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (student.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(student.RoleId.Value);
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

            // Set the UserId for the student
            student.UserId = user.Id;

            // Create the student
            return await base.CreateAsync(student);
        }

        public override async Task<StudentEntity> UpdateAsync(StudentEntity student)
        {
            // Get the user associated with this student
            var user = await _context.Users.FindAsync(student.UserId);
            if (user != null)
            {
                // Check if username is unique (excluding current user)
                if (!string.IsNullOrEmpty(student.User?.Username) && student.User.Username != user.Username)
                {
                    var isUsernameUnique = await _userManager.IsUsernameUniqueAsync(student.User.Username, user.Id);
                    if (!isUsernameUnique)
                    {
                        throw new InvalidOperationException($"Username '{student.User.Username}' is already taken. Please choose a different username.");
                    }
                }

                // Update user properties
                user.FirstName = student.User?.FirstName ?? user.FirstName;
                user.LastName = student.User?.LastName ?? user.LastName;
                user.Email = student.User?.Email ?? user.Email;
                user.Username = student.User?.Username ?? user.Username;
                user.PhoneNumber = student.User?.PhoneNumber ?? user.PhoneNumber;
                user.Address = student.User?.Address ?? user.Address;
                user.UpdatedAt = DateTime.UtcNow;

                // Update password if provided
                if (!string.IsNullOrEmpty(student.User?.PasswordHash))
                {
                    user.PasswordHash = EnsurePasswordIsHashed(student.User.PasswordHash);
                }

                // Update the user
                _context.Users.Update(user);
            }

            // Set the UpdatedAt timestamp for the student
            student.UpdatedAt = DateTime.UtcNow;

            // Update the student
            return await base.UpdateAsync(student);
        }

        public async Task<StudentEntity?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Attendances)
                .Include(s => s.Class)
                    .ThenInclude(c => c.ClassSubjects)
                        .ThenInclude(cs => cs.Subject)
                            .ThenInclude(sub => sub.Staff)
                                .ThenInclude(staff => staff.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<IEnumerable<StudentSubject>> GetStudentSubjectsWithSubjectAsync(Guid studentId, string? academicYear = null)
        {
            IQueryable<StudentSubject> query = _context.StudentSubjects.AsNoTracking().Where(ss => ss.StudentId == studentId);
            if (!string.IsNullOrEmpty(academicYear))
                query = query.Where(ss => ss.AcademicYear == academicYear);
            return await query
                .Include(ss => ss.Subject)
                    .ThenInclude(s => s.Staff)
                        .ThenInclude(st => st!.User)
                .ToListAsync();
        }

        public async Task SetStudentSubjectsAsync(Guid studentId, string academicYear, List<Guid> subjectIds, List<Guid>? optionalSubjectIds = null)
        {
            var academicYearEntity = await _academicYearManager.GetAcademicYearByCodeAsync(academicYear)
                ?? (await _academicYearManager.GetCurrentAcademicYearAsync());
            var academicYearId = academicYearEntity?.Id ?? (await _context.AcademicYears.OrderByDescending(ay => ay.StartDate).FirstOrDefaultAsync())?.Id;
            if (!academicYearId.HasValue)
                throw new InvalidOperationException("No academic year found. Please create and set a current academic year first.");

            var existing = await _context.StudentSubjects
                .Where(ss => ss.StudentId == studentId && ss.AcademicYear == academicYear)
                .ToListAsync();

            var toRemove = existing.Where(ss => !subjectIds.Contains(ss.SubjectId)).ToList();
            var currentIds = existing.Select(ss => ss.SubjectId).ToList();
            var newIds = subjectIds.Where(id => !currentIds.Contains(id)).ToList();

            if (toRemove.Any())
                _context.StudentSubjects.RemoveRange(toRemove);

            foreach (var subjectId in newIds)
            {
                _context.StudentSubjects.Add(new StudentSubject
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    SubjectId = subjectId,
                    AcademicYearId = academicYearId.Value,
                    AcademicYear = academicYear,
                    IsOptional = optionalSubjectIds?.Contains(subjectId) ?? false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            foreach (var ss in existing.Where(ss => subjectIds.Contains(ss.SubjectId)))
            {
                var shouldBeOptional = optionalSubjectIds?.Contains(ss.SubjectId) ?? false;
                if (ss.IsOptional != shouldBeOptional)
                    ss.IsOptional = shouldBeOptional;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<object?> GetStudentAcademicInfoByYearAsync(Guid studentId, Guid academicYearId)
        {
            var academicYear = await _academicYearManager.GetAcademicYearByIdAsync(academicYearId);
            if (academicYear == null)
                return null;

            var student = await _context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null)
                return null;

            var classEnrollment = await _context.ClassEnrollments
                .AsNoTracking()
                .Include(ce => ce.Class)
                .Include(ce => ce.Student)
                .FirstOrDefaultAsync(ce => ce.StudentId == studentId && ce.AcademicYearId == academicYearId && ce.IsActive);

            var academicHistory = await _context.StudentAcademicHistories
                .AsNoTracking()
                .FirstOrDefaultAsync(ah => ah.StudentId == studentId && ah.AcademicYearId == academicYearId);

            return new
            {
                RegistrationNumber = student.RegistrationNumber,
                AdmissionNumber = student.AdmissionNumber,
                DateOfAdmission = student.AdmissionDate,
                PreviousSchoolName = student.PreviousSchoolName,
                AcademicYear = academicYear.YearCode,
                AcademicYearName = academicYear.YearName,
                ClassId = classEnrollment?.ClassId,
                ClassName = classEnrollment?.Class?.Name ?? "Not Assigned",
                RollNumber = academicHistory?.RollNumber ?? classEnrollment?.Student?.RollNumber ?? "Not Assigned",
                Section = academicHistory?.Section ?? classEnrollment?.Class?.Section ?? "",
                Grade = academicHistory?.Grade ?? classEnrollment?.Class?.GradeLevel ?? "",
                EnrollmentDate = classEnrollment?.EnrollmentDate,
                Status = classEnrollment?.Status ?? "Not Enrolled",
                OverallPercentage = academicHistory?.OverallPercentage,
                OverallGrade = academicHistory?.OverallGrade,
                ClassRank = academicHistory?.ClassRank,
                PromotionStatus = academicHistory?.PromotionStatus ?? "Pending"
            };
        }

        public override async Task<bool> DeleteAsync(Guid id)
        {
            var student = await _dbSet
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return false;

            // Store user reference before deleting student
            var user = student.User;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Delete related records first to avoid foreign key constraints

                // 1. Delete Class Enrollments (if table exists)
                try
                {
                    var classEnrollments = await _context.ClassEnrollments
                        .Where(ce => ce.StudentId == id)
                        .ToListAsync();
                    if (classEnrollments.Any())
                    {
                        _context.ClassEnrollments.RemoveRange(classEnrollments);
                    }
                }
                catch (Exception ex)
                {
                    // Table might not exist, continue
                    Console.WriteLine($"ClassEnrollments deletion skipped: {ex.Message}");
                }

                // 2. Delete Attendances (if table exists)
                try
                {
                    var attendances = await _context.Attendances
                        .Where(a => a.StudentId == id)
                        .ToListAsync();
                    if (attendances.Any())
                    {
                        _context.Attendances.RemoveRange(attendances);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attendances deletion skipped: {ex.Message}");
                }

                // 3. Delete Student Fees (if table exists)
                try
                {
                    var studentFees = await _context.StudentFees
                        .Where(sf => sf.StudentId == id)
                        .ToListAsync();
                    if (studentFees.Any())
                    {
                        _context.StudentFees.RemoveRange(studentFees);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"StudentFees deletion skipped: {ex.Message}");
                }

                // 4. Delete Student Leaves (if table exists)
                try
                {
                    var studentLeaves = await _context.StudentLeaves
                        .Where(sl => sl.StudentId == id)
                        .ToListAsync();
                    if (studentLeaves.Any())
                    {
                        _context.StudentLeaves.RemoveRange(studentLeaves);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"StudentLeaves deletion skipped: {ex.Message}");
                }

                // 5. Student Bus Assignments removed

                // 6. Delete Academic History records (if table exists)
                // Note: StudentAcademicHistory table might not exist yet
                // This will be implemented when the table is created

                // 7. Delete Library related records if they exist
                // Note: Add more related entities as needed

                // Save changes for related records deletion
                await _context.SaveChangesAsync();

                // 8. Delete student record
                _dbSet.Remove(student);
                await _context.SaveChangesAsync();

                // 9. Delete associated user if it exists
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }

                // Commit transaction
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Cannot delete student. Error occurred: {ex.Message}", ex);
            }
        }
    }
}
