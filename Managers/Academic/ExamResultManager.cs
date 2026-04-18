using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Academic
{
    public class ExamResultManager : GenericManager<ExamResult>, IExamResultManager
    {
        public ExamResultManager(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ExamResultDTO>> GetResultsByExamAsync(Guid examId)
        {
            var results = await _dbSet
                .Include(r => r.Exam)
                .ThenInclude(e => e.Subject)
                .Include(r => r.Exam)
                .ThenInclude(e => e.Class)
                .Include(r => r.Student)
                .ThenInclude(s => s.User)
                .Include(r => r.EnteredBy)
                .Include(r => r.UpdatedBy)
                .Where(r => r.ExamId == examId)
                .OrderBy(r => r.Student.User.FirstName)
                .ThenBy(r => r.Student.User.LastName)
                .ToListAsync();

            return results.Select(MapToDTO);
        }

        public async Task<IEnumerable<ExamResultDTO>> GetResultsByStudentAsync(Guid studentId)
        {
            var results = await _dbSet
                .Include(r => r.Exam)
                .ThenInclude(e => e.Subject)
                .Include(r => r.Exam)
                .ThenInclude(e => e.Class)
                .Include(r => r.Student)
                .ThenInclude(s => s.User)
                .Include(r => r.EnteredBy)
                .Include(r => r.UpdatedBy)
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.Exam.ExamDate)
                .ToListAsync();

            return results.Select(MapToDTO);
        }

        public async Task<IEnumerable<ExamResultDTO>> GetResultsByClassAsync(Guid classId)
        {
            var results = await _dbSet
                .Include(r => r.Exam)
                .ThenInclude(e => e.Subject)
                .Include(r => r.Exam)
                .ThenInclude(e => e.Class)
                .Include(r => r.Student)
                .ThenInclude(s => s.User)
                .Include(r => r.EnteredBy)
                .Include(r => r.UpdatedBy)
                .Where(r => r.Exam.ClassId == classId)
                .OrderByDescending(r => r.Exam.ExamDate)
                .ThenBy(r => r.Student.User.FirstName)
                .ToListAsync();

            return results.Select(MapToDTO);
        }

        public async Task<IEnumerable<ExamResultDTO>> GetResultsByStaffAsync(Guid staffId)
        {
            var results = await _dbSet
                .Include(r => r.Exam)
                .ThenInclude(e => e.Subject)
                .Include(r => r.Exam)
                .ThenInclude(e => e.Class)
                .Include(r => r.Student)
                .ThenInclude(s => s.User)
                .Include(r => r.EnteredBy)
                .Include(r => r.UpdatedBy)
                .Where(r => r.Exam.Subject.StaffId == staffId)
                .OrderByDescending(r => r.Exam.ExamDate)
                .ThenBy(r => r.Student.User.FirstName)
                .ToListAsync();

            return results.Select(MapToDTO);
        }

        public async Task<ExamResultDTO?> GetResultDTOByIdAsync(Guid id)
        {
            var result = await _dbSet
                .Include(r => r.Exam)
                .ThenInclude(e => e.Subject)
                .Include(r => r.Exam)
                .ThenInclude(e => e.Class)
                .Include(r => r.Student)
                .ThenInclude(s => s.User)
                .Include(r => r.EnteredBy)
                .Include(r => r.UpdatedBy)
                .FirstOrDefaultAsync(r => r.Id == id);

            return result != null ? MapToDTO(result) : null;
        }

        public async Task<ExamResultDTO?> GetStudentExamResultAsync(Guid examId, Guid studentId)
        {
            var result = await _dbSet
                .Include(r => r.Exam)
                .ThenInclude(e => e.Subject)
                .Include(r => r.Exam)
                .ThenInclude(e => e.Class)
                .Include(r => r.Student)
                .ThenInclude(s => s.User)
                .Include(r => r.EnteredBy)
                .Include(r => r.UpdatedBy)
                .FirstOrDefaultAsync(r => r.ExamId == examId && r.StudentId == studentId);

            return result != null ? MapToDTO(result) : null;
        }

        public async Task<ExamResultDTO> CreateResultAsync(CreateExamResultDTO createResultDto, Guid enteredByUserId)
        {
            // Get exam details for calculations
            var exam = await _context.Exams.FindAsync(createResultDto.ExamId);
            if (exam == null) throw new ArgumentException("Exam not found");

            var result = new ExamResult
            {
                Id = Guid.NewGuid(),
                ExamId = createResultDto.ExamId,
                StudentId = createResultDto.StudentId,
                AcademicYearId = exam.AcademicYearId,
                MarksObtained = createResultDto.IsAbsent ? 0 : createResultDto.MarksObtained,
                Remarks = createResultDto.Remarks,
                IsAbsent = createResultDto.IsAbsent,
                AbsentReason = createResultDto.AbsentReason,
                EnteredByUserId = enteredByUserId,
                CreatedAt = DateTime.UtcNow
            };

            // Calculate grade, percentage, and pass status
            if (!result.IsAbsent)
            {
                result.Grade = CalculateGrade(result.MarksObtained, exam.TotalMarks);
                result.Percentage = CalculatePercentage(result.MarksObtained, exam.TotalMarks);
                result.IsPass = IsPassingGrade(result.MarksObtained, exam.PassingMarks);
            }
            else
            {
                result.Grade = "AB";
                result.Percentage = 0;
                result.IsPass = false;
            }

            await _dbSet.AddAsync(result);
            await _context.SaveChangesAsync();

            return await GetResultDTOByIdAsync(result.Id) ?? throw new InvalidOperationException("Failed to create result");
        }

        public async Task<ExamResultDTO?> UpdateResultAsync(Guid id, UpdateExamResultDTO updateResultDto, Guid updatedByUserId)
        {
            var result = await _dbSet.Include(r => r.Exam).FirstOrDefaultAsync(r => r.Id == id);
            if (result == null) return null;

            result.MarksObtained = updateResultDto.IsAbsent ? 0 : updateResultDto.MarksObtained;
            result.Remarks = updateResultDto.Remarks;
            result.IsAbsent = updateResultDto.IsAbsent;
            result.AbsentReason = updateResultDto.AbsentReason;
            result.UpdatedByUserId = updatedByUserId;
            result.UpdatedAt = DateTime.UtcNow;

            // Recalculate grade, percentage, and pass status
            if (!result.IsAbsent)
            {
                result.Grade = CalculateGrade(result.MarksObtained, result.Exam.TotalMarks);
                result.Percentage = CalculatePercentage(result.MarksObtained, result.Exam.TotalMarks);
                result.IsPass = IsPassingGrade(result.MarksObtained, result.Exam.PassingMarks);
            }
            else
            {
                result.Grade = "AB";
                result.Percentage = 0;
                result.IsPass = false;
            }

            await _context.SaveChangesAsync();
            return await GetResultDTOByIdAsync(id);
        }

        public async Task<bool> DeleteResultAsync(Guid id)
        {
            var result = await _dbSet.FindAsync(id);
            if (result == null) return false;

            _dbSet.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }

        public string CalculateGrade(int marksObtained, int totalMarks)
        {
            var percentage = (decimal)marksObtained / totalMarks * 100;

            return percentage switch
            {
                >= 90 => "A+",
                >= 80 => "A",
                >= 70 => "B+",
                >= 60 => "B",
                >= 50 => "C+",
                >= 40 => "C",
                >= 33 => "D",
                _ => "F"
            };
        }

        public decimal CalculatePercentage(int marksObtained, int totalMarks)
        {
            if (totalMarks == 0) return 0;
            return Math.Round((decimal)marksObtained / totalMarks * 100, 2);
        }

        public bool IsPassingGrade(int marksObtained, int passingMarks)
        {
            return marksObtained >= passingMarks;
        }

        public async Task<bool> CanStaffAccessResultAsync(Guid resultId, Guid staffId)
        {
            return await _dbSet
                .AnyAsync(r => r.Id == resultId && r.Exam.Subject.StaffId == staffId);
        }

        public async Task<bool> CanStaffAccessExamResultsAsync(Guid examId, Guid staffId)
        {
            return await _context.Exams
                .AnyAsync(e => e.Id == examId && e.Subject.StaffId == staffId);
        }

        public async Task<IEnumerable<ExamResultDTO>> CreateBulkResultsAsync(BulkExamResultDTO bulkResultDto, Guid enteredByUserId)
        {
            var exam = await _context.Exams.FindAsync(bulkResultDto.ExamId);
            if (exam == null) throw new ArgumentException("Exam not found");

            var results = new List<ExamResult>();

            foreach (var studentResult in bulkResultDto.StudentResults)
            {
                var result = new ExamResult
                {
                    Id = Guid.NewGuid(),
                    ExamId = bulkResultDto.ExamId,
                    StudentId = studentResult.StudentId,
                    AcademicYearId = exam.AcademicYearId,
                    MarksObtained = studentResult.IsAbsent ? 0 : studentResult.MarksObtained,
                    Remarks = studentResult.Remarks,
                    IsAbsent = studentResult.IsAbsent,
                    AbsentReason = studentResult.AbsentReason,
                    EnteredByUserId = enteredByUserId,
                    CreatedAt = DateTime.UtcNow
                };

                // Calculate grade, percentage, and pass status
                if (!result.IsAbsent)
                {
                    result.Grade = CalculateGrade(result.MarksObtained, exam.TotalMarks);
                    result.Percentage = CalculatePercentage(result.MarksObtained, exam.TotalMarks);
                    result.IsPass = IsPassingGrade(result.MarksObtained, exam.PassingMarks);
                }
                else
                {
                    result.Grade = "AB";
                    result.Percentage = 0;
                    result.IsPass = false;
                }

                results.Add(result);
            }

            await _dbSet.AddRangeAsync(results);
            await _context.SaveChangesAsync();

            return await GetResultsByExamAsync(bulkResultDto.ExamId);
        }

        public async Task<ExamResultSummaryDTO?> GetExamSummaryAsync(Guid examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Subject)
                .Include(e => e.Class)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) return null;

            var results = await _dbSet
                .Where(r => r.ExamId == examId)
                .ToListAsync();

            var totalStudents = results.Count;
            var studentsAppeared = results.Count(r => !r.IsAbsent);
            var studentsAbsent = results.Count(r => r.IsAbsent);
            var studentsPassed = results.Count(r => r.IsPass);
            var studentsFailed = studentsAppeared - studentsPassed;

            var passPercentage = studentsAppeared > 0 ? (decimal)studentsPassed / studentsAppeared * 100 : 0;
            var averageMarks = studentsAppeared > 0 ? results.Where(r => !r.IsAbsent).Average(r => r.MarksObtained) : 0;
            var highestMarks = studentsAppeared > 0 ? results.Where(r => !r.IsAbsent).Max(r => r.MarksObtained) : 0;
            var lowestMarks = studentsAppeared > 0 ? results.Where(r => !r.IsAbsent).Min(r => r.MarksObtained) : 0;

            return new ExamResultSummaryDTO
            {
                ExamId = examId,
                ExamName = exam.Name,
                SubjectName = exam.Subject?.Name ?? string.Empty,
                ClassName = exam.Class?.Name ?? string.Empty,
                TotalStudents = totalStudents,
                StudentsAppeared = studentsAppeared,
                StudentsAbsent = studentsAbsent,
                StudentsPassed = studentsPassed,
                StudentsFailed = studentsFailed,
                PassPercentage = Math.Round(passPercentage, 2),
                AverageMarks = Math.Round((decimal)averageMarks, 2),
                HighestMarks = highestMarks,
                LowestMarks = lowestMarks
            };
        }

        private static ExamResultDTO MapToDTO(ExamResult result)
        {
            return new ExamResultDTO
            {
                Id = result.Id,
                MarksObtained = result.MarksObtained,
                Remarks = result.Remarks,
                Grade = result.Grade,
                Percentage = result.Percentage,
                IsPass = result.IsPass,
                IsAbsent = result.IsAbsent,
                AbsentReason = result.AbsentReason,
                ExamId = result.ExamId,
                StudentId = result.StudentId,
                EnteredByUserId = result.EnteredByUserId,
                UpdatedByUserId = result.UpdatedByUserId,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                ExamName = result.Exam?.Name ?? string.Empty,
                SubjectName = result.Exam?.Subject?.Name ?? string.Empty,
                ClassName = result.Exam?.Class?.Name ?? string.Empty,
                StudentName = result.Student?.User?.FullName ?? string.Empty,
                StudentRollNumber = result.Student?.RollNumber ?? string.Empty,
                EnteredByName = result.EnteredBy?.FullName ?? string.Empty,
                UpdatedByName = result.UpdatedBy?.FullName ?? string.Empty,
                TotalMarks = result.Exam?.TotalMarks ?? 0
            };
        }
    }
}
