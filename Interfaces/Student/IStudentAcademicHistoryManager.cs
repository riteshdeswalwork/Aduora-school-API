using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface IStudentAcademicHistoryManager : IGenericManager<StudentAcademicHistory>
    {
        Task<IEnumerable<StudentAcademicHistoryDTO>> GetStudentAcademicHistoryAsync(Guid studentId);
        Task<StudentAcademicHistoryDTO?> GetStudentAcademicHistoryByYearAsync(Guid studentId, Guid academicYearId);
        Task<IEnumerable<StudentAcademicHistoryDTO>> GetClassAcademicHistoryAsync(Guid classId, Guid academicYearId);
        Task<StudentAcademicHistoryDTO> CreateStudentAcademicHistoryAsync(CreateStudentAcademicHistoryDTO createDto);
        Task<StudentAcademicHistoryDTO?> UpdateStudentAcademicHistoryAsync(Guid id, UpdateStudentAcademicHistoryDTO updateDto);
        Task<bool> DeleteStudentAcademicHistoryAsync(Guid id);
        Task<bool> UpdateAcademicPerformanceAsync(Guid studentId, Guid academicYearId, AcademicPerformanceUpdateDTO performanceDto);
        Task<bool> UpdateAttendanceSummaryAsync(Guid studentId, Guid academicYearId, AttendanceSummaryDTO attendanceDto);
        Task<bool> UpdateFeeSummaryAsync(Guid studentId, Guid academicYearId, FeeSummaryDTO feeDto);
        Task<bool> ProcessStudentPromotionAsync(Guid studentId, Guid academicYearId, StudentPromotionDTO promotionDto);
        Task<bool> UpdatePromotionStatusAsync(Guid studentId, Guid academicYearId, string promotionStatus, string? remarks = null);
        Task<IEnumerable<StudentAcademicHistoryDTO>> GetStudentsForPromotionAsync(Guid classId, Guid academicYearId);
        Task<IEnumerable<StudentAcademicHistoryDTO>> GetPromotedStudentsAsync(Guid academicYearId);
        Task<IEnumerable<StudentAcademicHistoryDTO>> GetDetainedStudentsAsync(Guid academicYearId);
        Task<ClassPerformanceReportDTO> GetClassPerformanceReportAsync(Guid classId, Guid academicYearId);
        Task<StudentProgressReportDTO> GetStudentProgressReportAsync(Guid studentId);
        Task<AcademicYearStatisticsDTO> GetAcademicYearStatisticsAsync(Guid academicYearId);
        Task<IEnumerable<StudentAcademicHistoryDTO>> GetTopPerformersAsync(Guid academicYearId, int count = 10);
        Task<IEnumerable<StudentAcademicHistoryDTO>> GetStudentsNeedingAttentionAsync(Guid academicYearId);
        Task<bool> BulkCreateAcademicHistoryAsync(IEnumerable<CreateStudentAcademicHistoryDTO> createDtos);
        Task<bool> BulkUpdatePromotionStatusAsync(IEnumerable<BulkPromotionUpdateDTO> promotionUpdates);
        Task<bool> GenerateAcademicHistoryForNewYearAsync(Guid newAcademicYearId, Guid previousAcademicYearId);
        Task<bool> HasAcademicHistoryAsync(Guid studentId, Guid academicYearId);
        Task<bool> CanCreateAcademicHistoryAsync(Guid studentId, Guid academicYearId);
        Task<bool> CanDeleteAcademicHistoryAsync(Guid id);
    }
}
