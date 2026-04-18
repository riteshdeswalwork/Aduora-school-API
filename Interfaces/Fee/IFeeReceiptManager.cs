using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Fee
{
    public interface IFeeReceiptManager : IGenericManager<FeeReceipt>
    {
        Task<IEnumerable<FeeReceipt>> GetReceiptsByPaymentIdAsync(Guid paymentId);
        Task<IEnumerable<FeeReceipt>> GetReceiptsByStudentIdAsync(Guid studentId);
        Task<FeeReceipt?> GetReceiptByNumberAsync(string receiptNumber);
        Task<FeeReceipt?> GenerateReceiptAsync(Guid paymentId, Guid issuedById);
        Task<string> GenerateReceiptNumberAsync();

        Task<FeeReceipt?> RecordReprintAsync(Guid receiptId);
    }
}
