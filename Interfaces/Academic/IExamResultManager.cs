using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Academic
{
    public interface IExamResultManager : IGenericManager<ExamResult>
    {
        Task<IEnumerable<ExamResultDTO>> GetResultsByExamAsync(Guid examId);
        Task<IEnumerable<ExamResultDTO>> GetResultsByStudentAsync(Guid studentId);
        Task<IEnumerable<ExamResultDTO>> GetResultsByClassAsync(Guid classId);
        Task<IEnumerable<ExamResultDTO>> GetResultsByStaffAsync(Guid staffId);
        Task<ExamResultDTO?> GetResultDTOByIdAsync(Guid id);
        Task<ExamResultDTO?> GetStudentExamResultAsync(Guid examId, Guid studentId);
        Task<ExamResultDTO> CreateResultAsync(CreateExamResultDTO createResultDto, Guid enteredByUserId);
        Task<ExamResultDTO?> UpdateResultAsync(Guid id, UpdateExamResultDTO updateResultDto, Guid updatedByUserId);
        Task<bool> DeleteResultAsync(Guid id);
        Task<IEnumerable<ExamResultDTO>> CreateBulkResultsAsync(BulkExamResultDTO bulkResultDto, Guid enteredByUserId);
        Task<ExamResultSummaryDTO?> GetExamSummaryAsync(Guid examId);
        Task<bool> CanStaffAccessResultAsync(Guid resultId, Guid staffId);
        Task<bool> CanStaffAccessExamResultsAsync(Guid examId, Guid staffId);
        string CalculateGrade(int marksObtained, int totalMarks);
        decimal CalculatePercentage(int marksObtained, int totalMarks);
        bool IsPassingGrade(int marksObtained, int passingMarks);
    }
}
