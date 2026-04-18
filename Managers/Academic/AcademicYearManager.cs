using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Academic
{
    public class AcademicYearManager : GenericManager<AcademicYear>, IAcademicYearManager
    {
        public AcademicYearManager(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AcademicYearDTO>> GetAllAcademicYearsAsync()
        {
            var academicYears = await _dbSet
                .Include(ay => ay.CreatedBy)
                .OrderByDescending(ay => ay.StartDate)
                .ToListAsync();

            return academicYears.Select(MapToDTO);
        }

        public async Task<AcademicYearDTO?> GetAcademicYearByIdAsync(Guid id)
        {
            var academicYear = await _dbSet
                .Include(ay => ay.CreatedBy)
                .FirstOrDefaultAsync(ay => ay.Id == id);

            return academicYear != null ? MapToDTO(academicYear) : null;
        }

        public async Task<AcademicYearDTO?> GetCurrentAcademicYearAsync()
        {
            var currentYear = await _dbSet
                .Include(ay => ay.CreatedBy)
                .FirstOrDefaultAsync(ay => ay.IsCurrentYear && ay.IsActive);

            return currentYear != null ? MapToDTO(currentYear) : null;
        }

        public async Task<AcademicYearDTO?> GetAcademicYearByCodeAsync(string yearCode)
        {
            var academicYear = await _dbSet
                .Include(ay => ay.CreatedBy)
                .FirstOrDefaultAsync(ay => ay.YearCode == yearCode);

            return academicYear != null ? MapToDTO(academicYear) : null;
        }

        public async Task<AcademicYearDTO> CreateAcademicYearAsync(CreateAcademicYearDTO createDto, Guid createdByUserId)
        {
            // Validate unique year code
            if (!await IsAcademicYearCodeUniqueAsync(createDto.YearCode))
            {
                throw new InvalidOperationException($"Academic year code '{createDto.YearCode}' already exists.");
            }

            // Validate date range
            if (await HasOverlappingDatesAsync(createDto.StartDate, createDto.EndDate))
            {
                throw new InvalidOperationException("Academic year dates overlap with existing academic year.");
            }

            // If this is set as current year, unset other current years
            if (createDto.IsCurrentYear)
            {
                await UnsetCurrentAcademicYearsAsync();
            }

            var academicYear = new AcademicYear
            {
                Id = Guid.NewGuid(),
                YearCode = createDto.YearCode,
                YearName = createDto.YearName,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                IsCurrentYear = createDto.IsCurrentYear,
                IsActive = true,
                Description = createDto.Description,
                NumberOfTerms = createDto.NumberOfTerms,
                TermNames = createDto.TermNames,
                AdmissionStartDate = createDto.AdmissionStartDate,
                AdmissionEndDate = createDto.AdmissionEndDate,
                ExamStartDate = createDto.ExamStartDate,
                ExamEndDate = createDto.ExamEndDate,
                ResultDeclarationDate = createDto.ResultDeclarationDate,
                FinancialYear = createDto.FinancialYear,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _dbSet.Add(academicYear);
            await _context.SaveChangesAsync();

            return await GetAcademicYearByIdAsync(academicYear.Id) ?? throw new InvalidOperationException("Failed to create academic year.");
        }

        public async Task<AcademicYearDTO?> UpdateAcademicYearAsync(Guid id, UpdateAcademicYearDTO updateDto)
        {
            var academicYear = await _dbSet.FindAsync(id);
            if (academicYear == null) return null;

            // Update properties if provided
            if (!string.IsNullOrEmpty(updateDto.YearName))
                academicYear.YearName = updateDto.YearName;

            if (updateDto.StartDate.HasValue)
                academicYear.StartDate = updateDto.StartDate.Value;

            if (updateDto.EndDate.HasValue)
                academicYear.EndDate = updateDto.EndDate.Value;

            if (updateDto.IsCurrentYear.HasValue)
            {
                if (updateDto.IsCurrentYear.Value)
                {
                    await UnsetCurrentAcademicYearsAsync();
                }
                academicYear.IsCurrentYear = updateDto.IsCurrentYear.Value;
            }

            if (updateDto.IsActive.HasValue)
                academicYear.IsActive = updateDto.IsActive.Value;

            if (updateDto.Description != null)
                academicYear.Description = updateDto.Description;

            if (updateDto.NumberOfTerms.HasValue)
                academicYear.NumberOfTerms = updateDto.NumberOfTerms.Value;

            if (!string.IsNullOrEmpty(updateDto.TermNames))
                academicYear.TermNames = updateDto.TermNames;

            if (updateDto.AdmissionStartDate.HasValue)
                academicYear.AdmissionStartDate = updateDto.AdmissionStartDate;

            if (updateDto.AdmissionEndDate.HasValue)
                academicYear.AdmissionEndDate = updateDto.AdmissionEndDate;

            if (updateDto.ExamStartDate.HasValue)
                academicYear.ExamStartDate = updateDto.ExamStartDate;

            if (updateDto.ExamEndDate.HasValue)
                academicYear.ExamEndDate = updateDto.ExamEndDate;

            if (updateDto.ResultDeclarationDate.HasValue)
                academicYear.ResultDeclarationDate = updateDto.ResultDeclarationDate;

            if (!string.IsNullOrEmpty(updateDto.FinancialYear))
                academicYear.FinancialYear = updateDto.FinancialYear;

            academicYear.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetAcademicYearByIdAsync(id);
        }

        public async Task<bool> DeleteAcademicYearAsync(Guid id)
        {
            if (!await CanDeleteAcademicYearAsync(id))
                return false;

            var academicYear = await _dbSet.FindAsync(id);
            if (academicYear == null) return false;

            _dbSet.Remove(academicYear);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetCurrentAcademicYearAsync(Guid academicYearId)
        {
            var academicYear = await _dbSet.FindAsync(academicYearId);
            if (academicYear == null || !academicYear.IsActive) return false;

            await UnsetCurrentAcademicYearsAsync();
           
            academicYear.IsCurrentYear = true;
            academicYear.UpdatedAt = DateTime.UtcNow;
           
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateAcademicYearAsync(Guid academicYearId)
        {
            var academicYear = await _dbSet.FindAsync(academicYearId);
            if (academicYear == null) return false;

            academicYear.IsActive = true;
            academicYear.UpdatedAt = DateTime.UtcNow;
           
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAcademicYearAsync(Guid academicYearId)
        {
            var academicYear = await _dbSet.FindAsync(academicYearId);
            if (academicYear == null) return false;

            // Cannot deactivate current year
            if (academicYear.IsCurrentYear) return false;

            academicYear.IsActive = false;
            academicYear.UpdatedAt = DateTime.UtcNow;
           
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> StartNewAcademicYearAsync(Guid newAcademicYearId, Guid userId)
        {
            var newYear = await _dbSet.FindAsync(newAcademicYearId);
            if (newYear == null || !newYear.IsActive) return false;

            // Set as current year
            await SetCurrentAcademicYearAsync(newAcademicYearId);

            // TODO: Implement additional logic for starting new academic year
            // - Generate academic history records for continuing students
            // - Update class enrollments
            // - Initialize fee structures
            // - Create timetables

            return true;
        }

        public async Task<bool> CloseAcademicYearAsync(Guid academicYearId, Guid userId)
        {
            var academicYear = await _dbSet.FindAsync(academicYearId);
            if (academicYear == null) return false;

            // TODO: Implement academic year closure logic
            // - Finalize all academic records
            // - Generate final reports
            // - Process promotions
            // - Archive data

            academicYear.IsCurrentYear = false;
            academicYear.UpdatedAt = DateTime.UtcNow;
           
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AcademicYearSummaryDTO> GetAcademicYearSummaryAsync(Guid academicYearId)
        {
            var academicYear = await _dbSet.FindAsync(academicYearId);
            if (academicYear == null)
                throw new ArgumentException("Academic year not found");

            // Get statistics from related tables
            var totalStudents = await _context.StudentAcademicHistories
                .CountAsync(sah => sah.AcademicYearId == academicYearId && sah.IsActive);

            var totalClasses = await _context.StudentAcademicHistories
                .Where(sah => sah.AcademicYearId == academicYearId && sah.IsActive)
                .Select(sah => sah.ClassId)
                .Distinct()
                .CountAsync();

            var studentsPromoted = await _context.StudentAcademicHistories
                .CountAsync(sah => sah.AcademicYearId == academicYearId && sah.PromotionStatus == "Promoted");

            var studentsDetained = await _context.StudentAcademicHistories
                .CountAsync(sah => sah.AcademicYearId == academicYearId && sah.PromotionStatus == "Detained");

            var studentsTransferred = await _context.StudentAcademicHistories
                .CountAsync(sah => sah.AcademicYearId == academicYearId && sah.PromotionStatus == "Transferred");

            var averageAttendance = await _context.StudentAcademicHistories
                .Where(sah => sah.AcademicYearId == academicYearId && sah.IsActive && sah.AttendancePercentage.HasValue)
                .AverageAsync(sah => sah.AttendancePercentage) ?? 0;

            var averagePerformance = await _context.StudentAcademicHistories
                .Where(sah => sah.AcademicYearId == academicYearId && sah.IsActive && sah.OverallPercentage.HasValue)
                .AverageAsync(sah => sah.OverallPercentage) ?? 0;

            var totalFeeAmount = await _context.StudentAcademicHistories
                .Where(sah => sah.AcademicYearId == academicYearId && sah.IsActive && sah.TotalFeeAmount.HasValue)
                .SumAsync(sah => sah.TotalFeeAmount) ?? 0;

            var totalFeePaid = await _context.StudentAcademicHistories
                .Where(sah => sah.AcademicYearId == academicYearId && sah.IsActive && sah.FeePaid.HasValue)
                .SumAsync(sah => sah.FeePaid) ?? 0;

            var totalFeePending = totalFeeAmount - totalFeePaid;
            var feeCollectionPercentage = totalFeeAmount > 0 ? (totalFeePaid / totalFeeAmount) * 100 : 0;

            return new AcademicYearSummaryDTO
            {
                AcademicYearId = academicYear.Id,
                YearCode = academicYear.YearCode,
                YearName = academicYear.YearName,
                StartDate = academicYear.StartDate,
                EndDate = academicYear.EndDate,
                IsCurrentYear = academicYear.IsCurrentYear,
                TotalStudents = totalStudents,
                TotalClasses = totalClasses,
                TotalSubjects = 0, // TODO: Calculate from subjects
                TotalExams = 0, // TODO: Calculate from exams
                StudentsPromoted = studentsPromoted,
                StudentsDetained = studentsDetained,
                StudentsTransferred = studentsTransferred,
                AverageAttendancePercentage = averageAttendance,
                AverageAcademicPerformance = averagePerformance,
                TotalFeeCollected = totalFeePaid,
                TotalFeePending = totalFeePending,
                FeeCollectionPercentage = feeCollectionPercentage,
                IsAdmissionPeriodActive = academicYear.AdmissionStartDate <= DateTime.Now && academicYear.AdmissionEndDate >= DateTime.Now,
                IsExamPeriodActive = academicYear.ExamStartDate <= DateTime.Now && academicYear.ExamEndDate >= DateTime.Now,
                AreResultsDeclared = academicYear.ResultDeclarationDate <= DateTime.Now
            };
        }

        public async Task<IEnumerable<AcademicYearDTO>> GetAcademicYearsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var academicYears = await _dbSet
                .Include(ay => ay.CreatedBy)
                .Where(ay => ay.StartDate <= endDate && ay.EndDate >= startDate)
                .OrderByDescending(ay => ay.StartDate)
                .ToListAsync();

            return academicYears.Select(MapToDTO);
        }

        public async Task<bool> IsAcademicYearCodeUniqueAsync(string yearCode, Guid? excludeId = null)
        {
            var query = _dbSet.Where(ay => ay.YearCode == yearCode);

            if (excludeId.HasValue)
                query = query.Where(ay => ay.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        public async Task<bool> HasOverlappingDatesAsync(DateTime startDate, DateTime endDate, Guid? excludeId = null)
        {
            var query = _dbSet.Where(ay => ay.IsActive &&
                ((ay.StartDate <= startDate && ay.EndDate >= startDate) ||
                 (ay.StartDate <= endDate && ay.EndDate >= endDate) ||
                 (ay.StartDate >= startDate && ay.EndDate <= endDate)));

            if (excludeId.HasValue)
                query = query.Where(ay => ay.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> CanDeleteAcademicYearAsync(Guid academicYearId)
        {
            // Cannot delete if it's the current year
            var academicYear = await _dbSet.FindAsync(academicYearId);
            if (academicYear?.IsCurrentYear == true) return false;

            // Cannot delete if there are student academic histories
            var hasStudentHistories = await _context.StudentAcademicHistories
                .AnyAsync(sah => sah.AcademicYearId == academicYearId);

            return !hasStudentHistories;
        }

        private async Task UnsetCurrentAcademicYearsAsync()
        {
            var currentYears = await _dbSet
                .Where(ay => ay.IsCurrentYear)
                .ToListAsync();

            foreach (var year in currentYears)
            {
                year.IsCurrentYear = false;
                year.UpdatedAt = DateTime.UtcNow;
            }

            if (currentYears.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        private static AcademicYearDTO MapToDTO(AcademicYear academicYear)
        {
            return new AcademicYearDTO
            {
                Id = academicYear.Id,
                YearCode = academicYear.YearCode,
                YearName = academicYear.YearName,
                StartDate = academicYear.StartDate,
                EndDate = academicYear.EndDate,
                IsCurrentYear = academicYear.IsCurrentYear,
                IsActive = academicYear.IsActive,
                Description = academicYear.Description,
                NumberOfTerms = academicYear.NumberOfTerms,
                TermNames = academicYear.TermNames,
                AdmissionStartDate = academicYear.AdmissionStartDate,
                AdmissionEndDate = academicYear.AdmissionEndDate,
                ExamStartDate = academicYear.ExamStartDate,
                ExamEndDate = academicYear.ExamEndDate,
                ResultDeclarationDate = academicYear.ResultDeclarationDate,
                FinancialYear = academicYear.FinancialYear,
                CreatedByName = academicYear.CreatedBy?.FullName ?? string.Empty,
                CreatedAt = academicYear.CreatedAt,
                UpdatedAt = academicYear.UpdatedAt,
                IsCurrentlyActive = academicYear.IsCurrentlyActive,
                DurationInDays = academicYear.DurationInDays,
                TermNamesList = academicYear.GetTermNames()
            };
        }
    }
}
