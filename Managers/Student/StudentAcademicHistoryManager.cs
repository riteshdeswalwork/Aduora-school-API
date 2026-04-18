using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Student;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Student
{
    public class StudentAcademicHistoryManager : GenericManager<StudentAcademicHistory>, IStudentAcademicHistoryManager
    {
        public StudentAcademicHistoryManager(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StudentAcademicHistoryDTO>> GetStudentAcademicHistoryAsync(Guid studentId)
        {
            var histories = await _dbSet
                .Include(sah => sah.Student)
                    .ThenInclude(s => s.User)
                .Include(sah => sah.AcademicYear)
                .Include(sah => sah.Class)
                .Where(sah => sah.StudentId == studentId)
                .OrderByDescending(sah => sah.AcademicYear.StartDate)
                .ToListAsync();

            return histories.Select(MapToDTO);
        }

        public async Task<StudentAcademicHistoryDTO?> GetStudentAcademicHistoryByYearAsync(Guid studentId, Guid academicYearId)
        {
            var history = await _dbSet
                .Include(sah => sah.Student)
                    .ThenInclude(s => s.User)
                .Include(sah => sah.AcademicYear)
                .Include(sah => sah.Class)
                .FirstOrDefaultAsync(sah => sah.StudentId == studentId && sah.AcademicYearId == academicYearId);

            return history != null ? MapToDTO(history) : null;
        }

        public async Task<IEnumerable<StudentAcademicHistoryDTO>> GetClassAcademicHistoryAsync(Guid classId, Guid academicYearId)
        {
            var histories = await _dbSet
                .Include(sah => sah.Student)
                    .ThenInclude(s => s.User)
                .Include(sah => sah.AcademicYear)
                .Include(sah => sah.Class)
                .Where(sah => sah.ClassId == classId && sah.AcademicYearId == academicYearId)
                .OrderBy(sah => sah.RollNumber)
                .ToListAsync();

            return histories.Select(MapToDTO);
        }

        public async Task<StudentAcademicHistoryDTO> CreateStudentAcademicHistoryAsync(CreateStudentAcademicHistoryDTO createDto)
        {
            // Validate if history already exists
            if (await HasAcademicHistoryAsync(createDto.StudentId, createDto.AcademicYearId))
            {
                throw new InvalidOperationException("Academic history already exists for this student and academic year.");
            }

            var history = new StudentAcademicHistory
            {
                Id = Guid.NewGuid(),
                StudentId = createDto.StudentId,
                AcademicYearId = createDto.AcademicYearId,
                ClassId = createDto.ClassId,
                Grade = createDto.Grade,
                Section = createDto.Section,
                RollNumber = createDto.RollNumber,
                OverallPercentage = createDto.OverallPercentage,
                OverallGrade = createDto.OverallGrade,
                TotalMarksObtained = createDto.TotalMarksObtained,
                TotalMaxMarks = createDto.TotalMaxMarks,
                ClassRank = createDto.ClassRank,
                TotalStudentsInClass = createDto.TotalStudentsInClass,
                TotalWorkingDays = createDto.TotalWorkingDays,
                TotalPresentDays = createDto.TotalPresentDays,
                TotalAbsentDays = createDto.TotalAbsentDays,
                AttendancePercentage = createDto.AttendancePercentage,
                TotalFeeAmount = createDto.TotalFeeAmount,
                FeePaid = createDto.FeePaid,
                FeeBalance = createDto.FeeBalance,
                DiscountAmount = createDto.DiscountAmount,
                PromotionStatus = createDto.PromotionStatus,
                Remarks = createDto.Remarks,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbSet.Add(history);
            await _context.SaveChangesAsync();

            return await GetStudentAcademicHistoryByYearAsync(createDto.StudentId, createDto.AcademicYearId)
                ?? throw new InvalidOperationException("Failed to create academic history.");
        }

        public async Task<StudentAcademicHistoryDTO?> UpdateStudentAcademicHistoryAsync(Guid id, UpdateStudentAcademicHistoryDTO updateDto)
        {
            var history = await _dbSet.FindAsync(id);
            if (history == null) return null;

            // Update properties if provided
            if (!string.IsNullOrEmpty(updateDto.Grade))
                history.Grade = updateDto.Grade;

            if (!string.IsNullOrEmpty(updateDto.Section))
                history.Section = updateDto.Section;

            if (!string.IsNullOrEmpty(updateDto.RollNumber))
                history.RollNumber = updateDto.RollNumber;

            if (updateDto.OverallPercentage.HasValue)
                history.OverallPercentage = updateDto.OverallPercentage;

            if (!string.IsNullOrEmpty(updateDto.OverallGrade))
                history.OverallGrade = updateDto.OverallGrade;

            if (updateDto.TotalMarksObtained.HasValue)
                history.TotalMarksObtained = updateDto.TotalMarksObtained;

            if (updateDto.TotalMaxMarks.HasValue)
                history.TotalMaxMarks = updateDto.TotalMaxMarks;

            if (updateDto.ClassRank.HasValue)
                history.ClassRank = updateDto.ClassRank;

            if (updateDto.TotalStudentsInClass.HasValue)
                history.TotalStudentsInClass = updateDto.TotalStudentsInClass;

            if (updateDto.TotalWorkingDays.HasValue)
                history.TotalWorkingDays = updateDto.TotalWorkingDays;

            if (updateDto.TotalPresentDays.HasValue)
                history.TotalPresentDays = updateDto.TotalPresentDays;

            if (updateDto.TotalAbsentDays.HasValue)
                history.TotalAbsentDays = updateDto.TotalAbsentDays;

            if (updateDto.AttendancePercentage.HasValue)
                history.AttendancePercentage = updateDto.AttendancePercentage;

            if (updateDto.TotalFeeAmount.HasValue)
                history.TotalFeeAmount = updateDto.TotalFeeAmount;

            if (updateDto.FeePaid.HasValue)
                history.FeePaid = updateDto.FeePaid;

            if (updateDto.FeeBalance.HasValue)
                history.FeeBalance = updateDto.FeeBalance;

            if (updateDto.DiscountAmount.HasValue)
                history.DiscountAmount = updateDto.DiscountAmount;

            if (!string.IsNullOrEmpty(updateDto.PromotionStatus))
                history.PromotionStatus = updateDto.PromotionStatus;

            if (!string.IsNullOrEmpty(updateDto.NextClass))
                history.NextClass = updateDto.NextClass;

            if (updateDto.PromotionDate.HasValue)
                history.PromotionDate = updateDto.PromotionDate;

            if (updateDto.Remarks != null)
                history.Remarks = updateDto.Remarks;

            if (updateDto.TotalWarnings.HasValue)
                history.TotalWarnings = updateDto.TotalWarnings;

            if (updateDto.TotalSuspensions.HasValue)
                history.TotalSuspensions = updateDto.TotalSuspensions;

            if (updateDto.DisciplinaryRemarks != null)
                history.DisciplinaryRemarks = updateDto.DisciplinaryRemarks;

            if (updateDto.ExtracurricularActivities != null)
                history.ExtracurricularActivities = updateDto.ExtracurricularActivities;

            if (updateDto.AchievementsAndAwards != null)
                history.AchievementsAndAwards = updateDto.AchievementsAndAwards;

            if (updateDto.HealthRemarks != null)
                history.HealthRemarks = updateDto.HealthRemarks;

            if (updateDto.IsActive.HasValue)
                history.IsActive = updateDto.IsActive.Value;

            history.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetStudentAcademicHistoryByYearAsync(history.StudentId, history.AcademicYearId);
        }

        public async Task<bool> DeleteStudentAcademicHistoryAsync(Guid id)
        {
            if (!await CanDeleteAcademicHistoryAsync(id))
                return false;

            var history = await _dbSet.FindAsync(id);
            if (history == null) return false;

            _dbSet.Remove(history);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAcademicPerformanceAsync(Guid studentId, Guid academicYearId, AcademicPerformanceUpdateDTO performanceDto)
        {
            var history = await _dbSet.FirstOrDefaultAsync(sah => sah.StudentId == studentId && sah.AcademicYearId == academicYearId);
            if (history == null) return false;

            if (performanceDto.OverallPercentage.HasValue)
                history.OverallPercentage = performanceDto.OverallPercentage;

            if (!string.IsNullOrEmpty(performanceDto.OverallGrade))
                history.OverallGrade = performanceDto.OverallGrade;

            if (performanceDto.TotalMarksObtained.HasValue)
                history.TotalMarksObtained = performanceDto.TotalMarksObtained;

            if (performanceDto.TotalMaxMarks.HasValue)
                history.TotalMaxMarks = performanceDto.TotalMaxMarks;

            if (performanceDto.ClassRank.HasValue)
                history.ClassRank = performanceDto.ClassRank;

            if (performanceDto.TotalStudentsInClass.HasValue)
                history.TotalStudentsInClass = performanceDto.TotalStudentsInClass;

            history.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAttendanceSummaryAsync(Guid studentId, Guid academicYearId, AttendanceSummaryDTO attendanceDto)
        {
            var history = await _dbSet.FirstOrDefaultAsync(sah => sah.StudentId == studentId && sah.AcademicYearId == academicYearId);
            if (history == null) return false;

            if (attendanceDto.TotalWorkingDays.HasValue)
                history.TotalWorkingDays = attendanceDto.TotalWorkingDays;

            if (attendanceDto.TotalPresentDays.HasValue)
                history.TotalPresentDays = attendanceDto.TotalPresentDays;

            if (attendanceDto.TotalAbsentDays.HasValue)
                history.TotalAbsentDays = attendanceDto.TotalAbsentDays;

            if (attendanceDto.AttendancePercentage.HasValue)
                history.AttendancePercentage = attendanceDto.AttendancePercentage;

            history.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateFeeSummaryAsync(Guid studentId, Guid academicYearId, FeeSummaryDTO feeDto)
        {
            var history = await _dbSet.FirstOrDefaultAsync(sah => sah.StudentId == studentId && sah.AcademicYearId == academicYearId);
            if (history == null) return false;

            if (feeDto.TotalFeeAmount.HasValue)
                history.TotalFeeAmount = feeDto.TotalFeeAmount;

            if (feeDto.FeePaid.HasValue)
                history.FeePaid = feeDto.FeePaid;

            if (feeDto.FeeBalance.HasValue)
                history.FeeBalance = feeDto.FeeBalance;

            if (feeDto.DiscountAmount.HasValue)
                history.DiscountAmount = feeDto.DiscountAmount;

            history.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ProcessStudentPromotionAsync(Guid studentId, Guid academicYearId, StudentPromotionDTO promotionDto)
        {
            var history = await _dbSet.FirstOrDefaultAsync(sah => sah.StudentId == studentId && sah.AcademicYearId == academicYearId);
            if (history == null) return false;

            history.PromotionStatus = promotionDto.PromotionStatus;
            history.NextClass = promotionDto.NextClass;
            history.PromotionDate = promotionDto.PromotionDate ?? DateTime.UtcNow;
            history.Remarks = promotionDto.Remarks;
            history.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePromotionStatusAsync(Guid studentId, Guid academicYearId, string promotionStatus, string? remarks = null)
        {
            var history = await _dbSet.FirstOrDefaultAsync(sah => sah.StudentId == studentId && sah.AcademicYearId == academicYearId);
            if (history == null) return false;

            history.PromotionStatus = promotionStatus;
            if (remarks != null)
                history.Remarks = remarks;
            history.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<StudentAcademicHistoryDTO>> GetStudentsForPromotionAsync(Guid classId, Guid academicYearId)
        {
            var histories = await _dbSet
                .Include(sah => sah.Student)
                    .ThenInclude(s => s.User)
                .Include(sah => sah.AcademicYear)
                .Include(sah => sah.Class)
                .Where(sah => sah.ClassId == classId && sah.AcademicYearId == academicYearId && sah.IsActive)
                .OrderBy(sah => sah.RollNumber)
                .ToListAsync();

            return histories.Select(MapToDTO);
        }

        public async Task<IEnumerable<StudentAcademicHistoryDTO>> GetPromotedStudentsAsync(Guid academicYearId)
        {
            var histories = await _dbSet
                .Include(sah => sah.Student)
                    .ThenInclude(s => s.User)
                .Include(sah => sah.AcademicYear)
                .Include(sah => sah.Class)
                .Where(sah => sah.AcademicYearId == academicYearId && sah.PromotionStatus == "Promoted")
                .OrderBy(sah => sah.Class.Name)
                .ThenBy(sah => sah.RollNumber)
                .ToListAsync();

            return histories.Select(MapToDTO);
        }

        public async Task<IEnumerable<StudentAcademicHistoryDTO>> GetDetainedStudentsAsync(Guid academicYearId)
        {
            var histories = await _dbSet
                .Include(sah => sah.Student)
                    .ThenInclude(s => s.User)
                .Include(sah => sah.AcademicYear)
                .Include(sah => sah.Class)
                .Where(sah => sah.AcademicYearId == academicYearId && sah.PromotionStatus == "Detained")
                .OrderBy(sah => sah.Class.Name)
                .ThenBy(sah => sah.RollNumber)
                .ToListAsync();

            return histories.Select(MapToDTO);
        }

        // Validation methods
        public async Task<bool> HasAcademicHistoryAsync(Guid studentId, Guid academicYearId)
        {
            return await _dbSet.AnyAsync(sah => sah.StudentId == studentId && sah.AcademicYearId == academicYearId);
        }

        public async Task<bool> CanCreateAcademicHistoryAsync(Guid studentId, Guid academicYearId)
        {
            return !await HasAcademicHistoryAsync(studentId, academicYearId);
        }

        public async Task<bool> CanDeleteAcademicHistoryAsync(Guid id)
        {
            var history = await _dbSet.FindAsync(id);
            if (history == null) return false;

            // Add business logic for deletion validation
            // For example, cannot delete if there are related performance records
            return true;
        }

        // Mapping method
        private static StudentAcademicHistoryDTO MapToDTO(StudentAcademicHistory history)
        {
            return new StudentAcademicHistoryDTO
            {
                Id = history.Id,
                StudentId = history.StudentId,
                StudentName = history.Student?.User?.FullName ?? string.Empty,
                StudentAdmissionNumber = history.Student?.AdmissionNumber ?? string.Empty,
                AcademicYearId = history.AcademicYearId,
                AcademicYearCode = history.AcademicYear?.YearCode ?? string.Empty,
                AcademicYearName = history.AcademicYear?.YearName ?? string.Empty,
                ClassId = history.ClassId,
                ClassName = history.Class?.Name ?? string.Empty,
                Grade = history.Grade,
                Section = history.Section,
                RollNumber = history.RollNumber,
                OverallPercentage = history.OverallPercentage,
                OverallGrade = history.OverallGrade,
                TotalMarksObtained = history.TotalMarksObtained,
                TotalMaxMarks = history.TotalMaxMarks,
                ClassRank = history.ClassRank,
                TotalStudentsInClass = history.TotalStudentsInClass,
                TotalWorkingDays = history.TotalWorkingDays,
                TotalPresentDays = history.TotalPresentDays,
                TotalAbsentDays = history.TotalAbsentDays,
                AttendancePercentage = history.AttendancePercentage,
                TotalFeeAmount = history.TotalFeeAmount,
                FeePaid = history.FeePaid,
                FeeBalance = history.FeeBalance,
                DiscountAmount = history.DiscountAmount,
                PromotionStatus = history.PromotionStatus,
                NextClass = history.NextClass,
                PromotionDate = history.PromotionDate,
                Remarks = history.Remarks,
                TotalWarnings = history.TotalWarnings,
                TotalSuspensions = history.TotalSuspensions,
                DisciplinaryRemarks = history.DisciplinaryRemarks,
                ExtracurricularActivities = history.ExtracurricularActivities,
                AchievementsAndAwards = history.AchievementsAndAwards,
                HealthRemarks = history.HealthRemarks,
                IsActive = history.IsActive,
                CreatedAt = history.CreatedAt,
                UpdatedAt = history.UpdatedAt,
                IsPromoted = history.IsPromoted,
                IsDetained = history.IsDetained,
                ClassDisplay = history.ClassDisplay,
                FeeCompletionPercentage = history.FeeCompletionPercentage
            };
        }

        public async Task<ClassPerformanceReportDTO> GetClassPerformanceReportAsync(Guid classId, Guid academicYearId)
        {
            var histories = await _dbSet
                .Include(sah => sah.Student)
                    .ThenInclude(s => s.User)
                .Include(sah => sah.AcademicYear)
                .Include(sah => sah.Class)
                .Where(sah => sah.ClassId == classId && sah.AcademicYearId == academicYearId && sah.IsActive)
                .ToListAsync();

            if (!histories.Any())
                throw new ArgumentException("No academic history found for the specified class and academic year.");

            var classInfo = histories.First().Class;
            var academicYear = histories.First().AcademicYear;

            var totalStudents = histories.Count;
            var averagePercentage = histories.Where(h => h.OverallPercentage.HasValue).Average(h => h.OverallPercentage) ?? 0;
            var averageAttendance = histories.Where(h => h.AttendancePercentage.HasValue).Average(h => h.AttendancePercentage) ?? 0;

            var studentsPromoted = histories.Count(h => h.PromotionStatus == "Promoted");
            var studentsDetained = histories.Count(h => h.PromotionStatus == "Detained");
            var studentsTransferred = histories.Count(h => h.PromotionStatus == "Transferred");

            var totalFeeAmount = histories.Where(h => h.TotalFeeAmount.HasValue).Sum(h => h.TotalFeeAmount) ?? 0;
            var totalFeePaid = histories.Where(h => h.FeePaid.HasValue).Sum(h => h.FeePaid) ?? 0;
            var feeCollectionPercentage = totalFeeAmount > 0 ? (totalFeePaid / totalFeeAmount) * 100 : 0;

            var topPerformers = histories
                .Where(h => h.OverallPercentage.HasValue)
                .OrderByDescending(h => h.OverallPercentage)
                .Take(5)
                .Select(MapToDTO)
                .ToList();

            var studentsNeedingAttention = histories
                .Where(h => (h.OverallPercentage.HasValue && h.OverallPercentage < 60) ||
                           (h.AttendancePercentage.HasValue && h.AttendancePercentage < 75))
                .Select(MapToDTO)
                .ToList();

            return new ClassPerformanceReportDTO
            {
                ClassId = classId,
                ClassName = classInfo.Name,
                AcademicYearId = academicYearId,
                AcademicYearCode = academicYear.YearCode,
                TotalStudents = totalStudents,
                AveragePercentage = averagePercentage,
                AverageAttendance = averageAttendance,
                StudentsPromoted = studentsPromoted,
                StudentsDetained = studentsDetained,
                StudentsTransferred = studentsTransferred,
                FeeCollectionPercentage = feeCollectionPercentage,
                TopPerformers = topPerformers,
                StudentsNeedingAttention = studentsNeedingAttention
            };
        }

        // TODO: Implement remaining methods from interface
        public Task<StudentProgressReportDTO> GetStudentProgressReportAsync(Guid studentId)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<AcademicYearStatisticsDTO> GetAcademicYearStatisticsAsync(Guid academicYearId)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<IEnumerable<StudentAcademicHistoryDTO>> GetTopPerformersAsync(Guid academicYearId, int count = 10)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<IEnumerable<StudentAcademicHistoryDTO>> GetStudentsNeedingAttentionAsync(Guid academicYearId)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<bool> BulkCreateAcademicHistoryAsync(IEnumerable<CreateStudentAcademicHistoryDTO> createDtos)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<bool> BulkUpdatePromotionStatusAsync(IEnumerable<BulkPromotionUpdateDTO> promotionUpdates)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }

        public Task<bool> GenerateAcademicHistoryForNewYearAsync(Guid newAcademicYearId, Guid previousAcademicYearId)
        {
            throw new NotImplementedException("Will be implemented in next iteration");
        }
    }
}