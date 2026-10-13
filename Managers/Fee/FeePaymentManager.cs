using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Fee;
using sms.api.Interfaces.Shared;
using sms.api.Interfaces.Student;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Services;
using sms.api.Services.Data;
using sms.api.Utils;
using sms.api.Controllers.Fee;

namespace sms.api.Managers.Fee
{
    public class FeePaymentManager : GenericManager<FeePayment>, IFeePaymentManager
    {
        private readonly IStudentFeeManager _studentFeeManager;
        private readonly INotificationOrchestrator? _notify;
        private readonly ISqlDataAccess _sqlDataAccess;

        public FeePaymentManager(ApplicationDbContext context, IStudentFeeManager studentFeeManager, ISqlDataAccess sqlDataAccess, INotificationOrchestrator? notify = null) : base(context)
        {
            _studentFeeManager = studentFeeManager;
            _sqlDataAccess = sqlDataAccess;
            _notify = notify;
        }

        // Flat row shapes returned by dbo.sp_FeePayments_GetPaged.
        private sealed class FeePaymentPagedRow
        {
            public Guid Id { get; set; }
            public Guid StudentFeeId { get; set; }
            public Guid AcademicYearId { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDate { get; set; }
            public string? PaymentMethod { get; set; }
            public string? TransactionReference { get; set; }
            public string? ReceiptNumber { get; set; }
            public string? Remarks { get; set; }
            public Guid ReceivedById { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public bool IsVoided { get; set; }
            public DateTime? VoidedAtUtc { get; set; }
            public Guid? VoidedById { get; set; }
            public string? VoidReason { get; set; }
            public Guid? SF_Id { get; set; }
            public Guid? SF_StudentId { get; set; }
            public decimal SF_TotalAmount { get; set; }
            public decimal SF_PaidAmount { get; set; }
            public decimal SF_OutstandingAmount { get; set; }
            public DateTime? SF_DueDate { get; set; }
            public string? SF_Status { get; set; }
            public Guid? Stu_Id { get; set; }
            public string? Stu_FirstName { get; set; }
            public string? Stu_LastName { get; set; }
            public Guid? FS_Id { get; set; }
            public string? FS_Name { get; set; }
            public decimal FS_Amount { get; set; }
            public Guid? FS_ClassId { get; set; }
            public Guid? Class_Id { get; set; }
            public string? Class_Name { get; set; }
            public Guid? RB_Id { get; set; }
            public string? RB_FirstName { get; set; }
            public string? RB_LastName { get; set; }
        }

        private sealed class FeeReceiptRow
        {
            public Guid Id { get; set; }
            public Guid PaymentId { get; set; }
            public Guid AcademicYearId { get; set; }
            public string? ReceiptNumber { get; set; }
            public DateTime IssueDate { get; set; }
            public decimal Amount { get; set; }
            public string? Description { get; set; }
            public Guid IssuedById { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public int ReprintCount { get; set; }
            public DateTime? LastReprintAtUtc { get; set; }
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
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@AcademicYearId", academicYearId.HasValue && academicYearId.Value != Guid.Empty ? academicYearId : null);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            parameters.Add("@ClassId", classId.HasValue && classId.Value != Guid.Empty ? classId : null);

            using var connection = await _sqlDataAccess.CreateOpenConnectionAsync();
            using var multi = await connection.QueryMultipleAsync(
                "dbo.sp_FeePayments_GetPaged", parameters, commandType: System.Data.CommandType.StoredProcedure);

            var totalCount = await multi.ReadSingleAsync<int>();
            var paymentRows = (await multi.ReadAsync<FeePaymentPagedRow>()).ToList();
            var receiptRows = (await multi.ReadAsync<FeeReceiptRow>()).ToList();
            var receiptsByPayment = receiptRows.ToLookup(r => r.PaymentId);

            var items = paymentRows.Select(r => new FeePayment
            {
                Id = r.Id,
                StudentFeeId = r.StudentFeeId,
                AcademicYearId = r.AcademicYearId,
                Amount = r.Amount,
                PaymentDate = r.PaymentDate,
                PaymentMethod = r.PaymentMethod ?? string.Empty,
                TransactionReference = r.TransactionReference ?? string.Empty,
                ReceiptNumber = r.ReceiptNumber ?? string.Empty,
                Remarks = r.Remarks ?? string.Empty,
                ReceivedById = r.ReceivedById,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                IsVoided = r.IsVoided,
                VoidedAtUtc = r.VoidedAtUtc,
                VoidedById = r.VoidedById,
                VoidReason = r.VoidReason ?? string.Empty,
                StudentFee = r.SF_Id.HasValue ? new StudentFee
                {
                    Id = r.SF_Id.Value,
                    StudentId = r.SF_StudentId ?? Guid.Empty,
                    TotalAmount = r.SF_TotalAmount,
                    PaidAmount = r.SF_PaidAmount,
                    OutstandingAmount = r.SF_OutstandingAmount,
                    DueDate = r.SF_DueDate ?? default,
                    Status = r.SF_Status ?? string.Empty,
                    Student = r.Stu_Id.HasValue ? new sms.api.Models.Student
                    {
                        Id = r.Stu_Id.Value,
                        User = new sms.api.Models.User { FirstName = r.Stu_FirstName ?? string.Empty, LastName = r.Stu_LastName ?? string.Empty },
                    } : null,
                    FeeStructure = r.FS_Id.HasValue ? new FeeStructure
                    {
                        Id = r.FS_Id.Value,
                        Name = r.FS_Name ?? string.Empty,
                        Amount = r.FS_Amount,
                        ClassId = r.FS_ClassId ?? Guid.Empty,
                        Class = r.Class_Id.HasValue ? new sms.api.Models.Class { Id = r.Class_Id.Value, Name = r.Class_Name ?? string.Empty } : null,
                    } : null,
                } : null,
                ReceivedBy = r.RB_Id.HasValue ? new sms.api.Models.User { Id = r.RB_Id.Value, FirstName = r.RB_FirstName ?? string.Empty, LastName = r.RB_LastName ?? string.Empty } : null,
                Receipts = receiptsByPayment[r.Id].Select(rc => new FeeReceipt
                {
                    Id = rc.Id,
                    PaymentId = rc.PaymentId,
                    AcademicYearId = rc.AcademicYearId,
                    ReceiptNumber = rc.ReceiptNumber ?? string.Empty,
                    IssueDate = rc.IssueDate,
                    Amount = rc.Amount,
                    Description = rc.Description ?? string.Empty,
                    IssuedById = rc.IssuedById,
                    CreatedAt = rc.CreatedAt,
                    UpdatedAt = rc.UpdatedAt,
                    ReprintCount = rc.ReprintCount,
                    LastReprintAtUtc = rc.LastReprintAtUtc,
                }).ToList(),
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<FeePayment>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
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

                var studentUserId = studentFee.Student?.UserId;
                if (studentUserId == null || studentUserId == Guid.Empty)
                    studentUserId = await NotificationEvents.GetStudentUserIdAsync(_context, studentFee.StudentId);

                var feeName = studentFee.FeeStructure?.Name ?? "fee";
                await NotificationEvents.NotifyUsersAsync(
                    _notify,
                    studentUserId.HasValue ? new[] { studentUserId.Value } : Enumerable.Empty<Guid>(),
                    "Fee payment received",
                    $"Payment of {request.Amount:N2} collected for {feeName}. Receipt: {payment.ReceiptNumber}.",
                    "fee.collected",
                    "fees",
                    NotificationChannels.All,
                    payment.Id,
                    "FeePayment",
                    "success",
                    smsBody: $"Fee {request.Amount:N0} received for {feeName}.",
                    idempotencyKey: $"fee.collected:{payment.Id}",
                    includeAdmins: true);

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
                var payment = await _dbSet
                    .Include(fp => fp.StudentFee)
                        .ThenInclude(sf => sf!.Student)
                    .FirstOrDefaultAsync(fp => fp.Id == paymentId);
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

                var studentUserId = payment.StudentFee?.Student?.UserId;
                if ((!studentUserId.HasValue || studentUserId == Guid.Empty) && payment.StudentFee != null)
                    studentUserId = await NotificationEvents.GetStudentUserIdAsync(_context, payment.StudentFee.StudentId);

                await NotificationEvents.NotifyUsersAsync(
                    _notify,
                    studentUserId.HasValue ? new[] { studentUserId.Value } : Enumerable.Empty<Guid>(),
                    "Fee payment voided",
                    $"A fee payment of {payment.Amount:N2} was voided. {payment.VoidReason}".Trim(),
                    "fee.voided",
                    "fees",
                    NotificationChannels.All,
                    payment.Id,
                    "FeePayment",
                    "warning",
                    idempotencyKey: $"fee.voided:{payment.Id}",
                    includeAdmins: true);

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
