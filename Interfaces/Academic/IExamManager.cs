using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Academic
{
    public interface IExamManager : IGenericManager<Exam>
    {
        Task<IEnumerable<ExamDTO>> GetExamsByClassAsync(Guid classId);
        Task<IEnumerable<ExamDTO>> GetExamsBySubjectAsync(Guid subjectId);
        Task<IEnumerable<ExamDTO>> GetExamsByStaffAsync(Guid staffId);
        Task<IEnumerable<ExamDTO>> GetUpcomingExamsAsync();
        Task<IEnumerable<ExamDTO>> GetExamsByAcademicYearAsync(string academicYear);
        Task<ExamDTO?> GetExamDTOByIdAsync(Guid id);
        Task<ExamDTO> CreateExamAsync(CreateExamDTO createExamDto, Guid createdByUserId);
        Task<ExamDTO?> UpdateExamAsync(Guid id, UpdateExamDTO updateExamDto);
        Task<bool> DeleteExamAsync(Guid id);
        Task<bool> CanStaffAccessExamAsync(Guid examId, Guid staffId);
    }
}
