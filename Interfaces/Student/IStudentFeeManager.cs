using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Student
{
    public interface IStudentFeeManager : IGenericManager<StudentFee>
    {
        Task<IEnumerable<StudentFee>> GetStudentFeesByStudentIdAsync(Guid studentId);
        Task<IEnumerable<StudentFee>> GetStudentFeesByClassIdAsync(Guid classId);
        Task<IEnumerable<StudentFee>> GetOutstandingFeesAsync();
        Task<IEnumerable<StudentFee>> GetOverdueFeesAsync();
        Task<StudentFee?> GetStudentFeeByStudentAndStructureAsync(Guid studentId, Guid feeStructureId);

        Task<IReadOnlyList<StudentFee>> GetStudentFeesForStudentAndStructureAsync(Guid studentId, Guid feeStructureId);

        Task<StudentFee?> GetStudentFeeLineForBillingPeriodAsync(Guid studentId, Guid feeStructureId, int billingYear, int billingMonth, Guid? feeStructureLineId);

        Task<IReadOnlyList<StudentFee>> GetStudentFeeLinesForBillingPeriodAsync(Guid studentId, Guid feeStructureId, int billingYear, int billingMonth, Guid? feeStructureLineId);
        Task<bool> UpdateOutstandingAmountAsync(Guid studentFeeId);
        Task<decimal> GetTotalOutstandingByClassAsync(Guid classId);
        Task<decimal> GetTotalCollectedByClassAsync(Guid classId);
        Task<IEnumerable<StudentFee>> GetFeesByStatusAsync(string status);

        /// <summary>
        /// For every active enrollment in the academic year, creates a monthly StudentFee row for each active Monthly fee structure
        /// of that class when no row exists for the same student, structure, and calendar month (matched on due date month).
        /// </summary>
        Task<GenerateMonthlyDemandsResultDto> GenerateMissingMonthlyDemandsAsync(Guid academicYearId, int year, int month);

        Task<GenerateMonthlyDemandsResultDto> GenerateInstallmentDemandsAsync(Guid academicYearId, Guid feeStructureId);

        Task<IReadOnlyList<FeeReportRowDto>> GetFeeReportAsync(Guid? academicYearId, Guid? classId, DateTime? startDate, DateTime? endDate);

        Task<IReadOnlyList<FeeDefaulterRowDto>> GetFeeDefaultersAsync(Guid academicYearId, Guid? classId, int? minDaysOverdue);

        Task RecalculateStudentFeeOutstandingAsync(Guid studentFeeId);
    }
}
