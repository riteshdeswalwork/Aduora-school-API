using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Controllers.Fee;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Fee
{
    public interface IFeePaymentManager : IGenericManager<FeePayment>
    {
        Task<PagedResult<FeePayment>> GetPaymentsPagedAsync(int pageNumber, int pageSize, Guid? academicYearId, DateTime? fromDate, DateTime? toDate, Guid? classId);
        Task<IEnumerable<FeePayment>> GetPaymentsByStudentFeeIdAsync(Guid studentFeeId);
        Task<IEnumerable<FeePayment>> GetPaymentsByStudentIdAsync(Guid studentId);
        Task<IEnumerable<FeePayment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<FeePayment>> GetPaymentsByReceivedByIdAsync(Guid receivedById);
        Task<decimal> GetTotalPaymentsByClassAsync(Guid classId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetTotalPaymentsByStudentAsync(Guid studentId);
        Task<FeePayment?> ProcessPaymentAsync(FeePayment payment);
        Task<string> GenerateReceiptNumberAsync();
        Task<FeeCollectionResult?> CollectFeePaymentAsync(CollectFeePaymentRequest request, Guid collectedById);

        Task<bool> VoidPaymentAsync(Guid paymentId, Guid voidedById, string? reason);

        Task<IReadOnlyList<FeeDayBookRowDto>> GetDayBookSummaryAsync(Guid? academicYearId, DateTime? fromDate, DateTime? toDate, Guid? classId);

        Task<IReadOnlyList<FeeDayBookDetailDto>> GetDayBookDetailAsync(Guid? academicYearId, DateTime? fromDate, DateTime? toDate, Guid? classId, Guid? receivedById);
    }

    public class FeeCollectionResult
    {
        public FeePayment Payment { get; set; } = new();
        public FeeReceipt? Receipt { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
