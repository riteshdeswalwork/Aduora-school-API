using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Fee;
using sms.api.Interfaces.Shared;
using sms.api.Interfaces.Student;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Utils;
using sms.api.Controllers.Fee;

namespace sms.api.Managers.Fee
{
    public class FeePaymentManager : GenericManager<FeePayment>, IFeePaymentManager
    {
        private readonly IStudentFeeManager _studentFeeManager;

        public FeePaymentManager(ApplicationDbContext context, IStudentFeeManager studentFeeManager) : base(context)
        {
            _studentFeeManager = studentFeeManager;
        }

        public override async Task<IEnumerable<FeePayment>> GetAllAsync()
        {
            return await _dbSet
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.Student)
                        .ThenInclude(s => s.User)
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                        .ThenInclude(fs => fs.Class)
                .Include(fp => fp.ReceivedBy)
                .Include(fp => fp.Receipts)
                .OrderByDescending(fp => fp.PaymentDate)
                .ToListAsync();
        }

        public override async Task<FeePayment> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.Student)
                        .ThenInclude(s => s.User)
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                        .ThenInclude(fs => fs.Class)
                .Include(fp => fp.ReceivedBy)
                .Include(fp => fp.Receipts)
                .FirstOrDefaultAsync(fp => fp.Id == id);
        }

        public async Task<PagedResult<FeePayment>> GetPaymentsPagedAsync(int pageNumber, int pageSize, Guid? academicYearId, DateTime? fromDate, DateTime? toDate, Guid? classId)
        {
            var query = _dbSet
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.Student)
                        .ThenInclude(s => s.User)
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                        .ThenInclude(fs => fs.Class)
                .Include(fp => fp.ReceivedBy)
                .Include(fp => fp.Receipts)
                .AsNoTracking()
                .OrderByDescending(fp => fp.PaymentDate)
                .AsQueryable();

            if (academicYearId.HasValue && academicYearId.Value != Guid.Empty)
                query = query.Where(fp => fp.AcademicYearId == academicYearId.Value);
            if (fromDate.HasValue)
                query = query.Where(fp => fp.PaymentDate.Date >= fromDate.Value.Date);
            if (toDate.HasValue)
                query = query.Where(fp => fp.PaymentDate.Date <= toDate.Value.Date);
            if (classId.HasValue && classId.Value != Guid.Empty)
                query = query.Where(fp => fp.StudentFee != null && fp.StudentFee.FeeStructure != null && fp.StudentFee.FeeStructure.ClassId == classId.Value);

            query = query.Where(fp => !fp.IsVoided);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<IEnumerable<FeePayment>> GetPaymentsByStudentFeeIdAsync(Guid studentFeeId)
        {
            return await _dbSet
                .Include(fp => fp.ReceivedBy)
                .Include(fp => fp.Receipts)
                .Where(fp => fp.StudentFeeId == studentFeeId)
                .OrderByDescending(fp => fp.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeePayment>> GetPaymentsByStudentIdAsync(Guid studentId)
        {
            return await _dbSet
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                .Include(fp => fp.ReceivedBy)
                .Include(fp => fp.Receipts)
                .Where(fp => fp.StudentFee.StudentId == studentId)
                .OrderByDescending(fp => fp.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeePayment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.Student)
                        .ThenInclude(s => s.User)
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                        .ThenInclude(fs => fs.Class)
                .Include(fp => fp.ReceivedBy)
                .Where(fp => fp.PaymentDate.Date >= fromDate.Date && fp.PaymentDate.Date <= toDate.Date)
                .OrderByDescending(fp => fp.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeePayment>> GetPaymentsByReceivedByIdAsync(Guid receivedById)
        {
            return await _dbSet
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.Student)
                        .ThenInclude(s => s.User)
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                .Where(fp => fp.ReceivedById == receivedById)
                .OrderByDescending(fp => fp.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentsByClassAsync(Guid classId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbSet
                .Where(fp => fp.StudentFee.FeeStructure.ClassId == classId);

            if (fromDate.HasValue)
                query = query.Where(fp => fp.PaymentDate.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(fp => fp.PaymentDate.Date <= toDate.Value.Date);

            return await query.SumAsync(fp => fp.Amount);
        }

        public async Task<decimal> GetTotalPaymentsByStudentAsync(Guid studentId)
        {
            return await _dbSet
                .Where(fp => !fp.IsVoided && fp.StudentFee.StudentId == studentId)
                .SumAsync(fp => fp.Amount);
        }

        public async Task<FeePayment?> ProcessPaymentAsync(FeePayment payment)
        {
            // Generate receipt number if not provided
            if (string.IsNullOrEmpty(payment.ReceiptNumber))
            {
                payment.ReceiptNumber = await GenerateReceiptNumberAsync();
            }

            // Create the payment
            var createdPayment = await CreateAsync(payment);

            await _studentFeeManager.RecalculateStudentFeeOutstandingAsync(payment.StudentFeeId);

            return createdPayment;
        }

        public async Task<string> GenerateReceiptNumberAsync()
        {
            var today = DateTime.UtcNow;
            var prefix = $"RCP{today:yyyyMM}";

            var lastReceipt = await _dbSet
                .Where(fp => fp.ReceiptNumber.StartsWith(prefix))
                .OrderByDescending(fp => fp.ReceiptNumber)
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

        public async Task<FeeCollectionResult?> CollectFeePaymentAsync(CollectFeePaymentRequest request, Guid collectedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get the student fee record
                var studentFee = await _context.StudentFees
                    .Include(sf => sf.Student)
                        .ThenInclude(s => s.User)
                    .Include(sf => sf.FeeStructure)
                        .ThenInclude(fs => fs.Class)
                    .FirstOrDefaultAsync(sf => sf.Id == request.StudentFeeId);

                if (studentFee == null)
                    return null;

                // Validate payment amount
                if (request.Amount <= 0 || request.Amount > studentFee.OutstandingAmount)
                    throw new ArgumentException("Invalid payment amount");

                // Create payment record
                var payment = new FeePayment
                {
                    Id = Guid.NewGuid(),
                    StudentFeeId = request.StudentFeeId,
                    AcademicYearId = studentFee.AcademicYearId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    TransactionReference = request.TransactionReference ?? string.Empty,
                    ReceiptNumber = string.Empty,
                    Remarks = request.Remarks ?? string.Empty,
                    PaymentDate = DateTime.UtcNow,
                    ReceivedById = collectedById,
                    CreatedAt = DateTime.UtcNow
                };

                // Add payment to database
                await _dbSet.AddAsync(payment);

                // Generate receipt if requested
                FeeReceipt? receipt = null;
                if (request.GenerateReceipt)
                {
                    var receiptNumber = await GenerateReceiptNumberAsync();
                    receipt = new FeeReceipt
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = payment.Id,
                        AcademicYearId = studentFee.AcademicYearId,
                        ReceiptNumber = receiptNumber,
                        IssueDate = DateTime.UtcNow,
                        IssuedById = collectedById,
                        Amount = request.Amount,
                        Description = $"Fee payment for {studentFee.FeeStructure?.Name ?? "Monthly Fee"}",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.FeeReceipts.AddAsync(receipt);
                }

                await _context.SaveChangesAsync();
                await _studentFeeManager.RecalculateStudentFeeOutstandingAsync(request.StudentFeeId);
                await transaction.CommitAsync();

                // Load the complete payment with all related data
                var completePayment = await GetByIdAsync(payment.Id);

                return new FeeCollectionResult
                {
                    Payment = completePayment,
                    Receipt = receipt,
                    Message = "Payment collected successfully",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error collecting payment: {ex.Message}", ex);
            }
        }

        public async Task<bool> VoidPaymentAsync(Guid paymentId, Guid voidedById, string? reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = await _dbSet.FirstOrDefaultAsync(fp => fp.Id == paymentId);
                if (payment == null || payment.IsVoided)
                    return false;

                payment.IsVoided = true;
                payment.VoidedAtUtc = DateTime.UtcNow;
                payment.VoidedById = voidedById;
                payment.VoidReason = string.IsNullOrWhiteSpace(reason) ? string.Empty : reason.Trim();
                payment.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(payment);
                await _context.SaveChangesAsync();
                await _studentFeeManager.RecalculateStudentFeeOutstandingAsync(payment.StudentFeeId);
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IReadOnlyList<FeeDayBookRowDto>> GetDayBookSummaryAsync(Guid? academicYearId, DateTime? fromDate, DateTime? toDate, Guid? classId)
        {
            IQueryable<FeePayment> q = _dbSet.AsNoTracking()
                .Where(fp => !fp.IsVoided)
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf!.FeeStructure);

            if (academicYearId.HasValue && academicYearId.Value != Guid.Empty)
                q = q.Where(fp => fp.AcademicYearId == academicYearId.Value);
            if (fromDate.HasValue)
                q = q.Where(fp => fp.PaymentDate.Date >= fromDate.Value.Date);
            if (toDate.HasValue)
                q = q.Where(fp => fp.PaymentDate.Date <= toDate.Value.Date);
            if (classId.HasValue && classId.Value != Guid.Empty)
                q = q.Where(fp => fp.StudentFee != null && fp.StudentFee.FeeStructure != null && fp.StudentFee.FeeStructure.ClassId == classId.Value);

            var rows = await q.ToListAsync();
            return rows
                .GroupBy(fp => new { Day = fp.PaymentDate.Date, fp.PaymentMethod })
                .Select(g => new FeeDayBookRowDto
                {
                    PaymentDate = g.Key.Day,
                    PaymentMethod = g.Key.PaymentMethod,
                    Amount = g.Sum(x => x.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.PaymentDate)
                .ThenBy(x => x.PaymentMethod)
                .ToList();
        }

        public async Task<IReadOnlyList<FeeDayBookDetailDto>> GetDayBookDetailAsync(Guid? academicYearId, DateTime? fromDate, DateTime? toDate, Guid? classId, Guid? receivedById)
        {
            IQueryable<FeePayment> q = _dbSet.AsNoTracking()
                .Where(fp => !fp.IsVoided)
                .Include(fp => fp.ReceivedBy)
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf!.Student)
                        .ThenInclude(s => s!.User)
                .Include(fp => fp.StudentFee)
                    .ThenInclude(sf => sf!.FeeStructure)
                        .ThenInclude(fs => fs!.Class);

            if (academicYearId.HasValue && academicYearId.Value != Guid.Empty)
                q = q.Where(fp => fp.AcademicYearId == academicYearId.Value);
            if (fromDate.HasValue)
                q = q.Where(fp => fp.PaymentDate.Date >= fromDate.Value.Date);
            if (toDate.HasValue)
                q = q.Where(fp => fp.PaymentDate.Date <= toDate.Value.Date);
            if (classId.HasValue && classId.Value != Guid.Empty)
                q = q.Where(fp => fp.StudentFee != null && fp.StudentFee.FeeStructure != null && fp.StudentFee.FeeStructure.ClassId == classId.Value);
            if (receivedById.HasValue && receivedById.Value != Guid.Empty)
                q = q.Where(fp => fp.ReceivedById == receivedById.Value);

            var list = await q
                .OrderByDescending(fp => fp.PaymentDate)
                .ToListAsync();

            return list.Select(fp =>
            {
                var u = fp.StudentFee?.Student?.User;
                var name = u == null ? "" : string.Join(" ", new[] { u.FirstName, u.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
                var rb = fp.ReceivedBy;
                var rbName = rb == null ? null : string.Join(" ", new[] { rb.FirstName, rb.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
                return new FeeDayBookDetailDto
                {
                    PaymentDate = fp.PaymentDate,
                    PaymentId = fp.Id,
                    Amount = fp.Amount,
                    PaymentMethod = fp.PaymentMethod,
                    StudentName = string.IsNullOrWhiteSpace(name) ? (fp.StudentFee?.Student?.RegistrationNumber ?? "") : name.Trim(),
                    ClassName = fp.StudentFee?.FeeStructure?.Class?.Name ?? "",
                    ReceivedById = fp.ReceivedById,
                    ReceivedByName = string.IsNullOrEmpty(rbName) ? fp.ReceivedBy?.Email : rbName,
                    IsVoided = fp.IsVoided
                };
            }).ToList();
        }
    }
}
