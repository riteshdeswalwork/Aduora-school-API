using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Academic
{
    public class ExamManager : GenericManager<Exam>, IExamManager
    {
        private readonly IAcademicYearManager _academicYearManager;

        public ExamManager(ApplicationDbContext context, IAcademicYearManager academicYearManager) : base(context)
        {
            _academicYearManager = academicYearManager;
        }

        public async Task<IEnumerable<ExamDTO>> GetExamsByClassAsync(Guid classId)
        {
            var exams = await _dbSet
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .Include(e => e.CreatedBy)
                .Where(e => e.ClassId == classId && e.IsActive)
                .OrderByDescending(e => e.ExamDate)
                .ToListAsync();

            return exams.Select(MapToDTO);
        }

        public async Task<IEnumerable<ExamDTO>> GetExamsBySubjectAsync(Guid subjectId)
        {
            var exams = await _dbSet
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .Include(e => e.CreatedBy)
                .Where(e => e.SubjectId == subjectId && e.IsActive)
                .OrderByDescending(e => e.ExamDate)
                .ToListAsync();

            return exams.Select(MapToDTO);
        }

        public async Task<IEnumerable<ExamDTO>> GetExamsByStaffAsync(Guid staffId)
        {
            var exams = await _dbSet
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .Include(e => e.CreatedBy)
                .Where(e => e.Subject.StaffId == staffId && e.IsActive)
                .OrderByDescending(e => e.ExamDate)
                .ToListAsync();

            return exams.Select(MapToDTO);
        }

        public async Task<IEnumerable<ExamDTO>> GetUpcomingExamsAsync()
        {
            var exams = await _dbSet
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .Include(e => e.CreatedBy)
                .Where(e => e.ExamDate > DateTime.Now && e.IsActive)
                .OrderBy(e => e.ExamDate)
                .ToListAsync();

            return exams.Select(MapToDTO);
        }

        public async Task<IEnumerable<ExamDTO>> GetExamsByAcademicYearAsync(string academicYear)
        {
            var exams = await _dbSet
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .Include(e => e.CreatedBy)
                .Where(e => e.AcademicYear == academicYear && e.IsActive)
                .OrderByDescending(e => e.ExamDate)
                .ToListAsync();

            return exams.Select(MapToDTO);
        }

        public async Task<ExamDTO?> GetExamDTOByIdAsync(Guid id)
        {
            var exam = await _dbSet
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .Include(e => e.CreatedBy)
                .FirstOrDefaultAsync(e => e.Id == id);

            return exam != null ? MapToDTO(exam) : null;
        }

        public async Task<ExamDTO> CreateExamAsync(CreateExamDTO createExamDto, Guid createdByUserId)
        {
            // Get AcademicYearId from AcademicYear string
            var academicYear = await _academicYearManager.GetAcademicYearByCodeAsync(createExamDto.AcademicYear);
            if (academicYear == null)
            {
                // If no specific academic year found, try to get current academic year
                academicYear = await _academicYearManager.GetCurrentAcademicYearAsync();
                if (academicYear == null)
                {
                    throw new InvalidOperationException("No academic year found. Please create an academic year first.");
                }
            }

            var exam = new Exam
            {
                Id = Guid.NewGuid(),
                Name = createExamDto.Name,
                Description = createExamDto.Description,
                ExamDate = createExamDto.ExamDate,
                StartTime = createExamDto.StartTime,
                EndTime = createExamDto.EndTime,
                TotalMarks = createExamDto.TotalMarks,
                PassingMarks = createExamDto.PassingMarks,
                ExamType = createExamDto.ExamType,
                AcademicYear = createExamDto.AcademicYear,
                AcademicYearId = academicYear.Id,
                Term = createExamDto.Term,
                ClassId = createExamDto.ClassId,
                SubjectId = createExamDto.SubjectId,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _dbSet.AddAsync(exam);
            await _context.SaveChangesAsync();

            return await GetExamDTOByIdAsync(exam.Id) ?? throw new InvalidOperationException("Failed to create exam");
        }

        public async Task<ExamDTO?> UpdateExamAsync(Guid id, UpdateExamDTO updateExamDto)
        {
            var exam = await _dbSet.FindAsync(id);
            if (exam == null) return null;

            exam.Name = updateExamDto.Name;
            exam.Description = updateExamDto.Description;
            exam.ExamDate = updateExamDto.ExamDate;
            exam.StartTime = updateExamDto.StartTime;
            exam.EndTime = updateExamDto.EndTime;
            exam.TotalMarks = updateExamDto.TotalMarks;
            exam.PassingMarks = updateExamDto.PassingMarks;
            exam.ExamType = updateExamDto.ExamType;
            exam.AcademicYear = updateExamDto.AcademicYear;
            exam.Term = updateExamDto.Term;
            exam.IsActive = updateExamDto.IsActive;
            exam.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetExamDTOByIdAsync(id);
        }

        public async Task<bool> DeleteExamAsync(Guid id)
        {
            var exam = await _dbSet.FindAsync(id);
            if (exam == null) return false;

            exam.IsActive = false;
            exam.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanStaffAccessExamAsync(Guid examId, Guid staffId)
        {
            return await _dbSet
                .AnyAsync(e => e.Id == examId && e.Subject.StaffId == staffId);
        }

        private static ExamDTO MapToDTO(Exam exam)
        {
            return new ExamDTO
            {
                Id = exam.Id,
                Name = exam.Name,
                Description = exam.Description,
                ExamDate = exam.ExamDate,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                TotalMarks = exam.TotalMarks,
                PassingMarks = exam.PassingMarks,
                ExamType = exam.ExamType,
                AcademicYear = exam.AcademicYear,
                Term = exam.Term,
                IsActive = exam.IsActive,
                ClassId = exam.ClassId,
                SubjectId = exam.SubjectId,
                CreatedByUserId = exam.CreatedByUserId,
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt,
                ClassName = exam.Class?.Name ?? string.Empty,
                SubjectName = exam.Subject?.Name ?? string.Empty,
                CreatedByName = exam.CreatedBy?.FullName ?? string.Empty
            };
        }
    }
}
