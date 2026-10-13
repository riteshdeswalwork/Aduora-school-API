using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Student;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Services.Data;
using sms.api.Utils;
using sms.api.Models;

namespace sms.api.Managers.Student
{
    public class StudentFeeManager : GenericManager<StudentFee>, IStudentFeeManager
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public StudentFeeManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        // Flat row shapes returned by dbo.sp_StudentFees_GetPaged.
        private sealed class StudentFeePagedRow
        {
            public Guid Id { get; set; }
            public Guid StudentId { get; set; }
            public Guid FeeStructureId { get; set; }
            public Guid? FeeStructureLineId { get; set; }
            public Guid AcademicYearId { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal Amount { get; set; }
            public decimal PaidAmount { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal OutstandingAmount { get; set; }
            public DateTime DueDate { get; set; }
            public string? Status { get; set; }
            public string? Remarks { get; set; }
            public int? BillingPeriodYear { get; set; }
            public int? BillingPeriodMonth { get; set; }
            public int? InstallmentSequence { get; set; }
            public string? Source { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public string? Stu_RollNumber { get; set; }
            public string? Stu_RegistrationNumber { get; set; }
            public string? Stu_FirstName { get; set; }
            public string? Stu_LastName { get; set; }
            public string? Stu_Email { get; set; }
            public string? FS_Name { get; set; }
            public decimal FS_Amount { get; set; }
            public Guid? FS_ClassId { get; set; }
            public string? Class_Name { get; set; }
        }

        private sealed class StudentFeePaymentRow
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
        }

        private sealed class StudentFeeDiscountRow
        {
            public Guid Id { get; set; }
            public Guid StudentFeeId { get; set; }
            public Guid AcademicYearId { get; set; }
            public string? DiscountType { get; set; }
            public decimal Amount { get; set; }
            public decimal Percentage { get; set; }
            public string? Reason { get; set; }
            public Guid ApprovedById { get; set; }
            public DateTime ApprovedDate { get; set; }
            public string? Remarks { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        public override async Task<IEnumerable<StudentFee>> GetAllAsync()
        {
            return await _dbSet
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.User)
                .Include(sf => sf.FeeStructure)
                    .ThenInclude(fs => fs.Class)
                .Include(sf => sf.Payments)
                .Include(sf => sf.Discounts)
                .OrderByDescending(sf => sf.CreatedAt)
                .ToListAsync();
        }

        public override async Task<StudentFee> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.User)
                .Include(sf => sf.FeeStructure)
                    .ThenInclude(fs => fs.Class)
                .Include(sf => sf.FeeStructureLine)
                .Include(sf => sf.Payments)
                .Include(sf => sf.Discounts)
                .FirstOrDefaultAsync(sf => sf.Id == id);
        }

        public async Task<IEnumerable<StudentFee>> GetStudentFeesByStudentIdAsync(Guid studentId)
        {
            return await _dbSet
                .Include(sf => sf.FeeStructure)
                    .ThenInclude(fs => fs.Class)
                .Include(sf => sf.Payments)
                .Include(sf => sf.Discounts)
                .Where(sf => sf.StudentId == studentId)
                .OrderByDescending(sf => sf.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentFee>> GetStudentFeesByClassIdAsync(Guid classId)
        {
            return await _dbSet
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.User)
                .Include(sf => sf.FeeStructure)
                .Include(sf => sf.Payments)
                .Include(sf => sf.Discounts)
                .Where(sf => sf.FeeStructure.ClassId == classId)
                .OrderBy(sf => sf.Student.User.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentFee>> GetOutstandingFeesAsync()
        {
            return await _dbSet
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.User)
                .Include(sf => sf.FeeStructure)
                    .ThenInclude(fs => fs.Class)
                .Where(sf => sf.OutstandingAmount > 0)
                .OrderByDescending(sf => sf.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentFee>> GetOverdueFeesAsync()
        {
            return await _dbSet
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.User)
                .Include(sf => sf.FeeStructure)
                    .ThenInclude(fs => fs.Class)
                .Where(sf => sf.OutstandingAmount > 0 && sf.DueDate < DateTime.UtcNow)
                .OrderByDescending(sf => sf.DueDate)
                .ToListAsync();
        }

        public async Task<StudentFee?> GetStudentFeeByStudentAndStructureAsync(Guid studentId, Guid feeStructureId)
        {
            var list = await GetStudentFeesForStudentAndStructureAsync(studentId, feeStructureId);
            return list.Count == 1 ? list[0] : list.FirstOrDefault();
        }

        public async Task<IReadOnlyList<StudentFee>> GetStudentFeesForStudentAndStructureAsync(Guid studentId, Guid feeStructureId)
        {
            return await _dbSet
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.User)
                .Include(sf => sf.FeeStructure)
                .Include(sf => sf.FeeStructureLine)
                .Include(sf => sf.Payments)
                .Include(sf => sf.Discounts)
                .Where(sf => sf.StudentId == studentId && sf.FeeStructureId == feeStructureId)
                .OrderByDescending(sf => sf.DueDate)
                .ThenByDescending(sf => sf.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<StudentFee>> GetStudentFeeLinesForBillingPeriodAsync(
            Guid studentId, Guid feeStructureId, int billingYear, int billingMonth, Guid? feeStructureLineId)
        {
            var q = _dbSet.Where(sf =>
                sf.StudentId == studentId
                && sf.FeeStructureId == feeStructureId
                && (
                    (sf.BillingPeriodYear == billingYear && sf.BillingPeriodMonth == billingMonth)
                    || (sf.BillingPeriodYear == null && sf.DueDate.Year == billingYear && sf.DueDate.Month == billingMonth)));
            if (feeStructureLineId.HasValue && feeStructureLineId.Value != Guid.Empty)
                q = q.Where(sf => sf.FeeStructureLineId == feeStructureLineId);
            return await q
                .Include(sf => sf.FeeStructure)
                .Include(sf => sf.FeeStructureLine)
                .OrderByDescending(sf => sf.CreatedAt)
                .ToListAsync();
        }

        public async Task<StudentFee?> GetStudentFeeLineForBillingPeriodAsync(Guid studentId, Guid feeStructureId, int billingYear, int billingMonth, Guid? feeStructureLineId)
        {
            var matches = await GetStudentFeeLinesForBillingPeriodAsync(studentId, feeStructureId, billingYear, billingMonth, feeStructureLineId);
            if (matches.Count == 0)
                return null;
            if (feeStructureLineId.HasValue && feeStructureLineId.Value != Guid.Empty)
                return matches[0];
            return matches.Count == 1 ? matches[0] : null;
        }

        public async Task<bool> UpdateOutstandingAmountAsync(Guid studentFeeId)
        {
            var exists = await _dbSet.AnyAsync(sf => sf.Id == studentFeeId);
            if (!exists) return false;
            await RecalculateStudentFeeOutstandingAsync(studentFeeId);
            return true;
        }

        public async Task<decimal> GetTotalOutstandingByClassAsync(Guid classId)
        {
            return await _dbSet
                .Where(sf => sf.FeeStructure.ClassId == classId)
                .SumAsync(sf => sf.OutstandingAmount);
        }

        public async Task<decimal> GetTotalCollectedByClassAsync(Guid classId)
        {
            return await _dbSet
                .Where(sf => sf.FeeStructure.ClassId == classId)
                .SumAsync(sf => sf.PaidAmount);
        }

        public async Task<IEnumerable<StudentFee>> GetFeesByStatusAsync(string status)
        {
            return await _dbSet
                .Include(sf => sf.Student)
                    .ThenInclude(s => s.User)
                .Include(sf => sf.FeeStructure)
                    .ThenInclude(fs => fs.Class)
                .Where(sf => sf.Status == status)
                .OrderByDescending(sf => sf.DueDate)
                .ToListAsync();
        }

        public async Task<PagedResult<StudentFee>> GetStudentFeesPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            Guid? classId,
            string? status,
            Guid? academicYearId,
            IEnumerable<Guid>? allowedClassIds = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            List<Guid>? classSet = allowedClassIds?.ToList();
            var tvp = new DataTable();
            tvp.Columns.Add("Id", typeof(Guid));
            if (classSet != null)
                foreach (var id in classSet) tvp.Rows.Add(id);

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@ClassId", classId.HasValue && classId.Value != Guid.Empty ? classId : null);
            parameters.Add("@Status", string.IsNullOrWhiteSpace(status) ? null : status.Trim());
            parameters.Add("@AcademicYearId", academicYearId.HasValue && academicYearId.Value != Guid.Empty ? academicYearId : null);
            parameters.Add("@HasAllowedClassFilter", classSet != null);
            parameters.Add("@AllowedClassIds", tvp.AsTableValuedParameter("dbo.GuidListType"));

            using var connection = await _sqlDataAccess.CreateOpenConnectionAsync();
            using var multi = await connection.QueryMultipleAsync(
                "dbo.sp_StudentFees_GetPaged", parameters, commandType: CommandType.StoredProcedure);

            var totalCount = await multi.ReadSingleAsync<int>();
            var feeRows = (await multi.ReadAsync<StudentFeePagedRow>()).ToList();
            var paymentRows = (await multi.ReadAsync<StudentFeePaymentRow>()).ToList();
            var discountRows = (await multi.ReadAsync<StudentFeeDiscountRow>()).ToList();
            var paymentsByFee = paymentRows.ToLookup(r => r.StudentFeeId);
            var discountsByFee = discountRows.ToLookup(r => r.StudentFeeId);

            var items = feeRows.Select(r => new StudentFee
            {
                Id = r.Id,
                StudentId = r.StudentId,
                FeeStructureId = r.FeeStructureId,
                FeeStructureLineId = r.FeeStructureLineId,
                AcademicYearId = r.AcademicYearId,
                TotalAmount = r.TotalAmount,
                Amount = r.Amount,
                PaidAmount = r.PaidAmount,
                DiscountAmount = r.DiscountAmount,
                OutstandingAmount = r.OutstandingAmount,
                DueDate = r.DueDate,
                Status = r.Status ?? string.Empty,
                Remarks = r.Remarks ?? string.Empty,
                BillingPeriodYear = r.BillingPeriodYear,
                BillingPeriodMonth = r.BillingPeriodMonth,
                InstallmentSequence = r.InstallmentSequence,
                Source = r.Source ?? string.Empty,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Student = new sms.api.Models.Student
                {
                    Id = r.StudentId,
                    RollNumber = r.Stu_RollNumber ?? string.Empty,
                    RegistrationNumber = r.Stu_RegistrationNumber ?? string.Empty,
                    User = new sms.api.Models.User { FirstName = r.Stu_FirstName ?? string.Empty, LastName = r.Stu_LastName ?? string.Empty, Email = r.Stu_Email ?? string.Empty },
                },
                FeeStructure = new FeeStructure
                {
                    Id = r.FeeStructureId,
                    Name = r.FS_Name ?? string.Empty,
                    Amount = r.FS_Amount,
                    ClassId = r.FS_ClassId ?? Guid.Empty,
                    Class = r.FS_ClassId.HasValue ? new sms.api.Models.Class { Id = r.FS_ClassId.Value, Name = r.Class_Name ?? string.Empty } : null,
                },
                Payments = paymentsByFee[r.Id].Select(p => new FeePayment
                {
                    Id = p.Id,
                    StudentFeeId = p.StudentFeeId,
                    AcademicYearId = p.AcademicYearId,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod ?? string.Empty,
                    TransactionReference = p.TransactionReference ?? string.Empty,
                    ReceiptNumber = p.ReceiptNumber ?? string.Empty,
                    Remarks = p.Remarks ?? string.Empty,
                    ReceivedById = p.ReceivedById,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsVoided = p.IsVoided,
                    VoidedAtUtc = p.VoidedAtUtc,
                    VoidedById = p.VoidedById,
                    VoidReason = p.VoidReason ?? string.Empty,
                }).ToList(),
                Discounts = discountsByFee[r.Id].Select(d => new FeeDiscount
                {
                    Id = d.Id,
                    StudentFeeId = d.StudentFeeId,
                    AcademicYearId = d.AcademicYearId,
                    DiscountType = d.DiscountType ?? string.Empty,
                    Amount = d.Amount,
                    Percentage = d.Percentage,
                    Reason = d.Reason ?? string.Empty,
                    ApprovedById = d.ApprovedById,
                    ApprovedDate = d.ApprovedDate,
                    Remarks = d.Remarks ?? string.Empty,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                }).ToList(),
            }).ToList();

            var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
            return new PagedResult<StudentFee>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        public async Task<GenerateMonthlyDemandsResultDto> GenerateMissingMonthlyDemandsAsync(Guid academicYearId, int year, int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Month must be 1–12.");

            var ay = await _context.Set<AcademicYear>().AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == academicYearId);
            if (ay == null)
                throw new InvalidOperationException("Academic year not found.");

            var firstOfMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59, DateTimeKind.Utc);
            if (lastOfMonth.Date < ay.StartDate.Date || firstOfMonth.Date > ay.EndDate.Date)
            {
                return new GenerateMonthlyDemandsResultDto
                {
                    AcademicYearId = academicYearId,
                    TargetYear = year,
                    TargetMonth = month,
                    Created = 0,
                    SkippedExisting = 0,
                    Message = "Target month is outside this academic session; nothing generated."
                };
            }

            var enrollments = await _context.ClassEnrollments
                .AsNoTracking()
                .Where(e => e.AcademicYearId == academicYearId && e.IsActive)
                .Select(e => new { e.StudentId, e.ClassId })
                .ToListAsync();

            var monthlyStructures = await _context.FeeStructures
                .AsNoTracking()
                .Where(fs => fs.AcademicYearId == academicYearId && fs.IsActive
                    && fs.FeeType.ToLower() == "monthly")
                .ToListAsync();

            var structureIds = monthlyStructures.Select(s => s.Id).ToList();
            var allLines = await _context.FeeStructureLines
                .AsNoTracking()
                .Where(l => structureIds.Contains(l.FeeStructureId) && l.IsActive)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();
            var linesByStructure = allLines
                .GroupBy(l => l.FeeStructureId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var created = 0;
            var skipped = 0;
            var dueDate = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59, DateTimeKind.Utc);
            var remark = $"Auto monthly demand — {year:D4}-{month:D2}";

            foreach (var en in enrollments)
            {
                foreach (var fs in monthlyStructures.Where(s => s.ClassId == en.ClassId))
                {
                    var lines = linesByStructure.GetValueOrDefault(fs.Id) ?? new List<FeeStructureLine>();
                    if (lines.Count > 0)
                    {
                        foreach (var line in lines)
                        {
                            var exists = await _dbSet.AnyAsync(sf =>
                                sf.StudentId == en.StudentId
                                && sf.FeeStructureId == fs.Id
                                && sf.FeeStructureLineId == line.Id
                                && (
                                    (sf.BillingPeriodYear == year && sf.BillingPeriodMonth == month)
                                    || (sf.BillingPeriodYear == null && sf.DueDate.Year == year && sf.DueDate.Month == month)));

                            if (exists)
                            {
                                skipped++;
                                continue;
                            }

                            var amt = line.Amount;
                            var disc = Math.Round(amt * (line.DefaultConcessionPercent / 100m), 2, MidpointRounding.AwayFromZero);
                            if (disc > amt) disc = amt;
                            var outstanding = amt - disc;
                            var lineRemark = $"{remark} · {line.Name}";
                            var sfNew = new StudentFee
                            {
                                Id = Guid.NewGuid(),
                                StudentId = en.StudentId,
                                FeeStructureId = fs.Id,
                                FeeStructureLineId = line.Id,
                                AcademicYearId = academicYearId,
                                TotalAmount = amt,
                                Amount = amt,
                                PaidAmount = 0,
                                DiscountAmount = disc,
                                OutstandingAmount = outstanding,
                                DueDate = dueDate,
                                Status = "Pending",
                                Remarks = lineRemark,
                                BillingPeriodYear = year,
                                BillingPeriodMonth = month,
                                InstallmentSequence = 1,
                                Source = "AutoDemand",
                                CreatedAt = DateTime.UtcNow
                            };
                            await _dbSet.AddAsync(sfNew);
                            created++;
                        }
                    }
                    else
                    {
                        var exists = await _dbSet.AnyAsync(sf =>
                            sf.StudentId == en.StudentId
                            && sf.FeeStructureId == fs.Id
                            && sf.FeeStructureLineId == null
                            && (
                                (sf.BillingPeriodYear == year && sf.BillingPeriodMonth == month)
                                || (sf.BillingPeriodYear == null && sf.DueDate.Year == year && sf.DueDate.Month == month)));

                        if (exists)
                        {
                            skipped++;
                            continue;
                        }

                        var amt = fs.Amount;
                        var sfNew = new StudentFee
                        {
                            Id = Guid.NewGuid(),
                            StudentId = en.StudentId,
                            FeeStructureId = fs.Id,
                            FeeStructureLineId = null,
                            AcademicYearId = academicYearId,
                            TotalAmount = amt,
                            Amount = amt,
                            PaidAmount = 0,
                            DiscountAmount = 0,
                            OutstandingAmount = amt,
                            DueDate = dueDate,
                            Status = "Pending",
                            Remarks = remark,
                            BillingPeriodYear = year,
                            BillingPeriodMonth = month,
                            InstallmentSequence = 1,
                            Source = "AutoDemand",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _dbSet.AddAsync(sfNew);
                        created++;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new GenerateMonthlyDemandsResultDto
            {
                AcademicYearId = academicYearId,
                TargetYear = year,
                TargetMonth = month,
                Created = created,
                SkippedExisting = skipped,
                Message = $"Created {created} demand(s); skipped {skipped} existing."
            };
        }

        public async Task<IReadOnlyList<FeeReportRowDto>> GetFeeReportAsync(Guid? academicYearId, Guid? classId, DateTime? startDate, DateTime? endDate)
        {
            var q = _dbSet
                .AsNoTracking()
                .Include(sf => sf.Student).ThenInclude(s => s!.User)
                .Include(sf => sf.FeeStructure).ThenInclude(fs => fs!.Class)
                .Include(sf => sf.FeeStructureLine)
                .AsQueryable();

            if (academicYearId.HasValue && academicYearId.Value != Guid.Empty)
                q = q.Where(sf => sf.AcademicYearId == academicYearId.Value);
            if (classId.HasValue && classId.Value != Guid.Empty)
                q = q.Where(sf => sf.FeeStructure != null && sf.FeeStructure.ClassId == classId.Value);
            if (startDate.HasValue)
                q = q.Where(sf => sf.DueDate.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                q = q.Where(sf => sf.DueDate.Date <= endDate.Value.Date);

            var list = await q
                .OrderBy(sf => sf.Student!.User != null ? sf.Student.User.FirstName : "")
                .ThenBy(sf => sf.DueDate)
                .ToListAsync();

            return list.Select(sf =>
            {
                var u = sf.Student?.User;
                var name = u == null ? "" : string.Join(" ", new[] { u.FirstName, u.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
                var head = sf.FeeStructure?.Name ?? "";
                if (sf.FeeStructureLine != null)
                    head = string.IsNullOrWhiteSpace(head) ? sf.FeeStructureLine.Name : $"{head} — {sf.FeeStructureLine.Name}";
                return new FeeReportRowDto
                {
                    StudentFeeId = sf.Id,
                    StudentId = sf.StudentId,
                    StudentName = string.IsNullOrWhiteSpace(name) ? (sf.Student?.RegistrationNumber ?? "") : name.Trim(),
                    ClassName = sf.FeeStructure?.Class?.Name ?? "",
                    FeeHeadName = head,
                    TotalAmount = sf.TotalAmount,
                    PaidAmount = sf.PaidAmount,
                    DiscountAmount = sf.DiscountAmount,
                    OutstandingAmount = sf.OutstandingAmount,
                    Status = sf.Status,
                    DueDate = sf.DueDate
                };
            }).ToList();
        }

        public async Task<IReadOnlyList<FeeDefaulterRowDto>> GetFeeDefaultersAsync(Guid academicYearId, Guid? classId, int? minDaysOverdue)
        {
            var today = DateTime.UtcNow.Date;
            var q = _dbSet
                .AsNoTracking()
                .Include(sf => sf.Student).ThenInclude(s => s!.User)
                .Include(sf => sf.FeeStructure).ThenInclude(fs => fs!.Class)
                .Include(sf => sf.FeeStructureLine)
                .Where(sf => sf.AcademicYearId == academicYearId && sf.OutstandingAmount > 0 && sf.DueDate.Date < today);

            if (classId.HasValue && classId.Value != Guid.Empty)
                q = q.Where(sf => sf.FeeStructure != null && sf.FeeStructure.ClassId == classId.Value);

            var list = await q.OrderByDescending(sf => sf.DueDate).ToListAsync();
            var result = new List<FeeDefaulterRowDto>();
            foreach (var sf in list)
            {
                var days = (today - sf.DueDate.Date).Days;
                if (minDaysOverdue.HasValue && days < minDaysOverdue.Value)
                    continue;
                var u = sf.Student?.User;
                var name = u == null ? "" : string.Join(" ", new[] { u.FirstName, u.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
                var headName = sf.FeeStructure?.Name ?? "";
                if (sf.FeeStructureLine != null)
                    headName = string.IsNullOrWhiteSpace(headName) ? sf.FeeStructureLine.Name : $"{headName} — {sf.FeeStructureLine.Name}";
                var bucket = days <= 30 ? "0-30" : days <= 60 ? "31-60" : days <= 90 ? "61-90" : "90+";
                result.Add(new FeeDefaulterRowDto
                {
                    StudentFeeId = sf.Id,
                    StudentId = sf.StudentId,
                    StudentName = string.IsNullOrWhiteSpace(name) ? (sf.Student?.RegistrationNumber ?? "") : name.Trim(),
                    ClassName = sf.FeeStructure?.Class?.Name ?? "",
                    FeeHeadName = headName,
                    OutstandingAmount = sf.OutstandingAmount,
                    DueDate = sf.DueDate,
                    DaysOverdue = days,
                    AgingBucket = bucket
                });
            }
            return result;
        }

        public async Task RecalculateStudentFeeOutstandingAsync(Guid studentFeeId)
        {
            var studentFee = await _dbSet
                .Include(sf => sf.Payments)
                .FirstOrDefaultAsync(sf => sf.Id == studentFeeId);
            if (studentFee == null)
                return;

            var totalPaid = studentFee.Payments?.Where(p => !p.IsVoided).Sum(p => p.Amount) ?? 0;
            studentFee.PaidAmount = totalPaid;
            studentFee.OutstandingAmount = studentFee.TotalAmount - studentFee.PaidAmount - studentFee.DiscountAmount;

            if (studentFee.OutstandingAmount <= 0)
                studentFee.Status = "Paid";
            else if (studentFee.PaidAmount > 0)
                studentFee.Status = "Partial";
            else if (studentFee.DueDate < DateTime.UtcNow)
                studentFee.Status = "Overdue";
            else
                studentFee.Status = "Pending";

            studentFee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<GenerateMonthlyDemandsResultDto> GenerateInstallmentDemandsAsync(Guid academicYearId, Guid feeStructureId)
        {
            var fs = await _context.FeeStructures
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == feeStructureId && f.AcademicYearId == academicYearId);
            if (fs == null)
                throw new InvalidOperationException("Fee structure not found for this academic year.");

            var n = fs.InstallmentCount ?? 1;
            if (n < 2)
            {
                return new GenerateMonthlyDemandsResultDto
                {
                    AcademicYearId = academicYearId,
                    TargetYear = 0,
                    TargetMonth = 0,
                    Created = 0,
                    SkippedExisting = 0,
                    Message = "Set InstallmentCount to 2 or more on the fee structure (Yearly / One-time)."
                };
            }

            var ft = fs.FeeType.ToLowerInvariant().Replace(" ", "");
            if (ft != "yearly" && ft != "one-time" && ft != "onetime")
            {
                return new GenerateMonthlyDemandsResultDto
                {
                    AcademicYearId = academicYearId,
                    TargetYear = 0,
                    TargetMonth = 0,
                    Created = 0,
                    SkippedExisting = 0,
                    Message = "Installment split applies to Yearly or One-time fee types only."
                };
            }

            var ay = await _context.Set<AcademicYear>().AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == academicYearId);
            if (ay == null)
                throw new InvalidOperationException("Academic year not found.");

            var studentIds = await _context.ClassEnrollments
                .AsNoTracking()
                .Where(e => e.AcademicYearId == academicYearId && e.IsActive && e.ClassId == fs.ClassId)
                .Select(e => e.StudentId)
                .Distinct()
                .ToListAsync();

            var created = 0;
            var skipped = 0;
            var total = fs.Amount;
            var baseEach = Math.Round(total / n, 2, MidpointRounding.AwayFromZero);
            var running = 0m;

            foreach (var studentId in studentIds)
            {
                running = 0;
                for (var seq = 1; seq <= n; seq++)
                {
                    var exists = await _dbSet.AnyAsync(sf =>
                        sf.StudentId == studentId
                        && sf.FeeStructureId == fs.Id
                        && sf.AcademicYearId == academicYearId
                        && sf.InstallmentSequence == seq
                        && sf.FeeStructureLineId == null
                        && sf.Source == "InstallmentPlan");

                    if (exists)
                    {
                        skipped++;
                        continue;
                    }

                    var amt = seq == n ? total - running : baseEach;
                    running += amt;
                    var sessionStart = ay.StartDate;
                    if (sessionStart.Kind == DateTimeKind.Unspecified)
                        sessionStart = DateTime.SpecifyKind(sessionStart, DateTimeKind.Utc);
                    var dueMonthStart = sessionStart.Date.AddMonths(seq - 1);
                    var dueDate = new DateTime(dueMonthStart.Year, dueMonthStart.Month,
                        DateTime.DaysInMonth(dueMonthStart.Year, dueMonthStart.Month), 23, 59, 59, DateTimeKind.Utc);

                    var sfNew = new StudentFee
                    {
                        Id = Guid.NewGuid(),
                        StudentId = studentId,
                        FeeStructureId = fs.Id,
                        FeeStructureLineId = null,
                        AcademicYearId = academicYearId,
                        TotalAmount = amt,
                        Amount = amt,
                        PaidAmount = 0,
                        DiscountAmount = 0,
                        OutstandingAmount = amt,
                        DueDate = dueDate,
                        Status = "Pending",
                        Remarks = $"Installment {seq}/{n} — {fs.Name}",
                        BillingPeriodYear = dueDate.Year,
                        BillingPeriodMonth = dueDate.Month,
                        InstallmentSequence = seq,
                        Source = "InstallmentPlan",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _dbSet.AddAsync(sfNew);
                    created++;
                }
            }

            await _context.SaveChangesAsync();

            return new GenerateMonthlyDemandsResultDto
            {
                AcademicYearId = academicYearId,
                TargetYear = ay.StartDate.Year,
                TargetMonth = ay.StartDate.Month,
                Created = created,
                SkippedExisting = skipped,
                Message = $"Created {created} instalment line(s); skipped {skipped} existing."
            };
        }
    }
}
