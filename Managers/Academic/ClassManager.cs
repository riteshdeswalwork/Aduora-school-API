using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Academic
{
    public class ClassManager : IClassManager
    {
        private readonly ApplicationDbContext _context;

        public ClassManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClassDTO>> GetAllClassesAsync(Guid? academicYearId = null)
        {
            var query = _context.Classes
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .Include(c => c.ClassSubjects)
                .ThenInclude(cs => cs.Subject)
                .ThenInclude(s => s.Staff)
                .ThenInclude(st => st.User)
                .AsQueryable();

            if (academicYearId.HasValue)
            {
                var academicYear = await _context.AcademicYears.FindAsync(academicYearId.Value);
                if (academicYear != null)
                {
                    var yearCode = academicYear.YearCode?.Trim() ?? string.Empty;
                    var yearName = academicYear.YearName?.Trim() ?? string.Empty;
                    var yearNameShort = yearName.Replace("Academic Year ", "").Replace("Academic Year", "").Trim();
                    var toMatch = new[] { yearCode, yearName, yearNameShort }.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
                    if (toMatch.Count > 0)
                        query = query.Where(c => c.AcademicYear != null && toMatch.Contains(c.AcademicYear.Trim()));
                }
            }

            var classes = await query.ToListAsync();

            return classes.Select(c => new ClassDTO
            {
                Id = c.Id,
                Name = c.Name,
                GradeLevel = c.GradeLevel,
                AcademicYear = c.AcademicYear,
                Section = c.Section,
                RoomNumber = c.RoomNumber,
                MaxCapacity = c.MaxCapacity,
                CurrentEnrollment = c.CurrentEnrollment,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher != null && c.Teacher.User != null ? $"{c.Teacher.User.FirstName} {c.Teacher.User.LastName}" : null,
                MonthlyFee = c.MonthlyFee,
                YearlyFee = c.YearlyFee,
                FeeType = c.FeeType,
                MinimumSubjects = c.MinimumSubjects,
                SubjectIds = c.ClassSubjects.Select(cs => cs.SubjectId).ToList(),
                Subjects = c.ClassSubjects.Select(cs => new SubjectDTO
                {
                    Id = cs.Subject.Id,
                    Name = cs.Subject.Name,
                    Code = cs.Subject.Code,
                    Description = cs.Subject.Description,
                    Credits = 1, // Default credits since Subject model doesn't have Credits
                    IsActive = cs.Subject.IsActive,
                    IsCompulsory = cs.IsCompulsory, // Get compulsory status from ClassSubject
                    DepartmentId = null, // No department relation
                    DepartmentName = null, // No department relation
                    StaffId = cs.Subject.StaffId,
                    StaffName = cs.Subject.Staff?.User != null ? $"{cs.Subject.Staff.User.FirstName} {cs.Subject.Staff.User.LastName}" : null,
                    CreatedAt = cs.Subject.CreatedAt,
                    UpdatedAt = cs.Subject.UpdatedAt
                }).ToList(),
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });
        }

        public async Task<IEnumerable<ClassDTO>> GetClassesByStaffIdAsync(Guid staffId)
        {
            // Debug logging
            Console.WriteLine($"Looking for classes with TeacherId: {staffId}");

            var classes = await _context.Classes
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .Where(c => c.TeacherId == staffId)
                .ToListAsync();

            return classes.Select(c => new ClassDTO
            {
                Id = c.Id,
                Name = c.Name,
                GradeLevel = c.GradeLevel,
                AcademicYear = c.AcademicYear,
                Section = c.Section,
                RoomNumber = c.RoomNumber,
                MaxCapacity = c.MaxCapacity,
                CurrentEnrollment = c.CurrentEnrollment,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher != null && c.Teacher.User != null ? $"{c.Teacher.User.FirstName} {c.Teacher.User.LastName}" : null,
                MonthlyFee = c.MonthlyFee,
                YearlyFee = c.YearlyFee,
                FeeType = c.FeeType,
                MinimumSubjects = c.MinimumSubjects,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });
        }

        public async Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(Guid teacherId)
        {
            return await _context.Classes
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<ClassDTO> GetClassByIdAsync(Guid id)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .Include(c => c.ClassSubjects)
                .ThenInclude(cs => cs.Subject)
                .ThenInclude(s => s.Staff)
                .ThenInclude(st => st.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classEntity == null)
            {
                return null;
            }

            return new ClassDTO
            {
                Id = classEntity.Id,
                Name = classEntity.Name,
                GradeLevel = classEntity.GradeLevel,
                AcademicYear = classEntity.AcademicYear,
                Section = classEntity.Section,
                RoomNumber = classEntity.RoomNumber,
                MaxCapacity = classEntity.MaxCapacity,
                CurrentEnrollment = classEntity.CurrentEnrollment,
                TeacherId = classEntity.TeacherId,
                TeacherName = classEntity.Teacher != null && classEntity.Teacher.User != null ? $"{classEntity.Teacher.User.FirstName} {classEntity.Teacher.User.LastName}" : null,
                MonthlyFee = classEntity.MonthlyFee,
                YearlyFee = classEntity.YearlyFee,
                FeeType = classEntity.FeeType,
                MinimumSubjects = classEntity.MinimumSubjects,
                SubjectIds = classEntity.ClassSubjects.Select(cs => cs.SubjectId).ToList(),
                Subjects = classEntity.ClassSubjects.Select(cs => new SubjectDTO
                {
                    Id = cs.Subject.Id,
                    Name = cs.Subject.Name,
                    Code = cs.Subject.Code,
                    Description = cs.Subject.Description,
                    Credits = 1, // Default credits since Subject model doesn't have Credits
                    IsActive = cs.Subject.IsActive,
                    IsCompulsory = cs.IsCompulsory, // Get compulsory status from ClassSubject
                    DepartmentId = null, // No department relation
                    DepartmentName = null, // No department relation
                    StaffId = cs.Subject.StaffId,
                    StaffName = cs.Subject.Staff?.User != null ? $"{cs.Subject.Staff.User.FirstName} {cs.Subject.Staff.User.LastName}" : null,
                    CreatedAt = cs.Subject.CreatedAt,
                    UpdatedAt = cs.Subject.UpdatedAt
                }).ToList(),
                CreatedAt = classEntity.CreatedAt,
                UpdatedAt = classEntity.UpdatedAt
            };
        }

        public async Task<ClassDTO> CreateClassAsync(CreateClassDTO classDto)
        {
            var classEntity = new Class
            {
                Id = Guid.NewGuid(),
                Name = classDto.Name,
                GradeLevel = classDto.GradeLevel,
                AcademicYear = classDto.AcademicYear,
                Section = classDto.Section,
                RoomNumber = classDto.RoomNumber,
                MaxCapacity = classDto.MaxCapacity,
                CurrentEnrollment = 0,
                IsActive = true, // Set explicitly
                TeacherId = classDto.TeacherId,
                MonthlyFee = classDto.MonthlyFee,
                YearlyFee = classDto.YearlyFee,
                FeeType = string.IsNullOrEmpty(classDto.FeeType) ? "Monthly" : classDto.FeeType,
                MinimumSubjects = classDto.MinimumSubjects,
                CreatedAt = DateTime.Now
            };

            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();

            // Resolve AcademicYearId for ClassSubjects (required by FK)
            var academicYearId = classDto.AcademicYearId
                ?? (await _context.AcademicYears.OrderByDescending(ay => ay.StartDate).FirstOrDefaultAsync())?.Id;

            // Add class subjects
            if (classDto.ClassSubjects.Any() && academicYearId.HasValue)
            {
                var classSubjects = classDto.ClassSubjects.Select(cs => new ClassSubject
                {
                    Id = Guid.NewGuid(),
                    ClassId = classEntity.Id,
                    SubjectId = cs.SubjectId,
                    AcademicYearId = academicYearId.Value,
                    IsCompulsory = cs.IsCompulsory,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.ClassSubjects.AddRange(classSubjects);
                await _context.SaveChangesAsync();
            }
            // Fallback for backward compatibility with SubjectIds
            else if (classDto.SubjectIds.Any() && academicYearId.HasValue)
            {
                var compulsoryIds = classDto.CompulsorySubjectIds ?? new List<Guid>();
                var classSubjects = classDto.SubjectIds.Select(subjectId => new ClassSubject
                {
                    Id = Guid.NewGuid(),
                    ClassId = classEntity.Id,
                    SubjectId = subjectId,
                    AcademicYearId = academicYearId.Value,
                    IsCompulsory = compulsoryIds.Contains(subjectId),
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.ClassSubjects.AddRange(classSubjects);
                await _context.SaveChangesAsync();
            }

            return await GetClassByIdAsync(classEntity.Id);
        }

        public async Task<ClassDTO> UpdateClassAsync(Guid id, UpdateClassDTO classDto)
        {
            var classEntity = await _context.Classes.FindAsync(id);

            if (classEntity == null)
            {
                return null;
            }

            classEntity.Name = classDto.Name;
            classEntity.GradeLevel = classDto.GradeLevel;
            classEntity.AcademicYear = classDto.AcademicYear;
            classEntity.Section = classDto.Section;
            classEntity.RoomNumber = classDto.RoomNumber;
            classEntity.MaxCapacity = classDto.MaxCapacity;
            classEntity.TeacherId = classDto.TeacherId;
            classEntity.MonthlyFee = classDto.MonthlyFee;
            classEntity.YearlyFee = classDto.YearlyFee;
            classEntity.FeeType = classDto.FeeType;
            classEntity.MinimumSubjects = classDto.MinimumSubjects;
            classEntity.UpdatedAt = DateTime.Now;

            // Update class subjects
            var existingClassSubjects = await _context.ClassSubjects
                .Where(cs => cs.ClassId == id)
                .ToListAsync();

            // Resolve AcademicYearId for new ClassSubjects (required by FK)
            var academicYearId = classDto.AcademicYearId
                ?? existingClassSubjects.FirstOrDefault()?.AcademicYearId
                ?? (await _context.AcademicYears.OrderByDescending(ay => ay.StartDate).FirstOrDefaultAsync())?.Id;

            _context.ClassSubjects.RemoveRange(existingClassSubjects);

            if (classDto.ClassSubjects.Any() && academicYearId.HasValue)
            {
                var newClassSubjects = classDto.ClassSubjects.Select(cs => new ClassSubject
                {
                    Id = Guid.NewGuid(),
                    ClassId = id,
                    SubjectId = cs.SubjectId,
                    AcademicYearId = academicYearId.Value,
                    IsCompulsory = cs.IsCompulsory,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.ClassSubjects.AddRange(newClassSubjects);
            }
            // Fallback for backward compatibility with SubjectIds
            else if (classDto.SubjectIds.Any() && academicYearId.HasValue)
            {
                var compulsoryIds = classDto.CompulsorySubjectIds ?? new List<Guid>();
                var newClassSubjects = classDto.SubjectIds.Select(subjectId => new ClassSubject
                {
                    Id = Guid.NewGuid(),
                    ClassId = id,
                    SubjectId = subjectId,
                    AcademicYearId = academicYearId.Value,
                    IsCompulsory = compulsoryIds.Contains(subjectId),
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.ClassSubjects.AddRange(newClassSubjects);
            }

            await _context.SaveChangesAsync();

            return await GetClassByIdAsync(id);
        }

        public async Task<bool> DeleteClassAsync(Guid id)
        {
            var classEntity = await _context.Classes.FindAsync(id);

            if (classEntity == null)
            {
                return false;
            }

            _context.Classes.Remove(classEntity);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
