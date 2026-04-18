using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Fee;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Fee
{
    public class FeeReceiptManager : GenericManager<FeeReceipt>, IFeeReceiptManager
    {

        public FeeReceiptManager(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<FeeReceipt>> GetAllAsync()
        {
            return await _dbSet
                .Include(fr => fr.Payment)
                    .ThenInclude(fp => fp.StudentFee)
                        .ThenInclude(sf => sf.Student)
                            .ThenInclude(s => s.User)
                .Include(fr => fr.Payment)
                    .ThenInclude(fp => fp.StudentFee)
                        .ThenInclude(sf => sf.FeeStructure)
                            .ThenInclude(fs => fs.Class)
                .Include(fr => fr.IssuedBy)
                .OrderByDescending(fr => fr.IssueDate)
                .ToListAsync();
        }

        public override async Task<FeeReceipt> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(fr => fr.Payment)
                    .ThenInclude(fp => fp.StudentFee)
                        .ThenInclude(sf => sf.Student)
                            .ThenInclude(s => s.User)
                .Include(fr => fr.Payment)
                    .ThenInclude(fp => fp.StudentFee)
                        .ThenInclude(sf => sf.FeeStructure)
                            .ThenInclude(fs => fs.Class)
                .Include(fr => fr.IssuedBy)
                .FirstOrDefaultAsync(fr => fr.Id == id);
        }

        public async Task<IEnumerable<FeeReceipt>> GetReceiptsByPaymentIdAsync(Guid paymentId)
        {
            return await _dbSet
                .Include(fr => fr.IssuedBy)
                .Where(fr => fr.PaymentId == paymentId)
                .OrderByDescending(fr => fr.IssueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeeReceipt>> GetReceiptsByStudentIdAsync(Guid studentId)
        {
            return await _dbSet
                .Include(fr => fr.Payment)
                    .ThenInclude(fp => fp.StudentFee)
                        .ThenInclude(sf => sf.FeeStructure)
                .Include(fr => fr.IssuedBy)
                .Where(fr => fr.Payment.StudentFee.StudentId == studentId)
                .OrderByDescending(fr => fr.IssueDate)
                .ToListAsync();
        }

        public async Task<FeeReceipt?> GetReceiptByNumberAsync(string receiptNumber)
        {
            return await _dbSet
                .Include(fr => fr.Payment)
                    .ThenInclude(fp => fp.StudentFee)
                        .ThenInclude(sf => sf.Student)
                            .ThenInclude(s => s.User)
                .Include(fr => fr.Payment)
                    .ThenInclude(fp => fp.StudentFee)
                        .ThenInclude(sf => sf.FeeStructure)
                            .ThenInclude(fs => fs.Class)
                .Include(fr => fr.IssuedBy)
                .FirstOrDefaultAsync(fr => fr.ReceiptNumber == receiptNumber);
        }

        public async Task<FeeReceipt?> GenerateReceiptAsync(Guid paymentId, Guid issuedById)
        {
            var payment = await _context.FeePayments
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.Student)
                        .ThenInclude(s => s.User)
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                .FirstOrDefaultAsync(fp => fp.Id == paymentId);

            if (payment == null || payment.IsVoided) return null;

            var receipt = new FeeReceipt
            {
                Id = Guid.NewGuid(),
                PaymentId = paymentId,
                AcademicYearId = payment.AcademicYearId,
                ReceiptNumber = await GenerateReceiptNumberAsync(),
                IssueDate = DateTime.UtcNow,
                Amount = payment.Amount,
                Description = $"Fee payment for {payment.StudentFee?.FeeStructure?.Name ?? "Fee"}",
                IssuedById = issuedById,
                CreatedAt = DateTime.UtcNow
            };

            return await CreateAsync(receipt);
        }

        public async Task<string> GenerateReceiptNumberAsync()
        {
            var today = DateTime.UtcNow;
            var prefix = $"RCP{today:yyyyMM}";
           
            var lastReceipt = await _dbSet
                .Where(fr => fr.ReceiptNumber.StartsWith(prefix))
                .OrderByDescending(fr => fr.ReceiptNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastReceipt != null && !string.IsNullOrEmpty(lastReceipt.ReceiptNumber))
            {
                var lastNumberStr = lastReceipt.ReceiptNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }

        public async Task<FeeReceipt?> RecordReprintAsync(Guid receiptId)
        {
            var receipt = await _dbSet.FirstOrDefaultAsync(fr => fr.Id == receiptId);
            if (receipt == null)
                return null;
            receipt.ReprintCount += 1;
            receipt.LastReprintAtUtc = DateTime.UtcNow;
            receipt.UpdatedAt = DateTime.UtcNow;
            return await UpdateAsync(receipt);
        }
    }
}
