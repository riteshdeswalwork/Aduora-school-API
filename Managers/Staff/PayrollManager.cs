using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Staff;
using sms.api.Models;
using sms.api.Models.DTOs;
using sms.api.Models.Enums;

namespace sms.api.Managers.Staff
{
    public class PayrollManager : IPayrollManager
    {
        private readonly ApplicationDbContext _context;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public PayrollManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<PayrollComponentDefinitionDto>> GetComponentsAsync()
        {
            var list = await _context.PayrollComponentDefinitions
                .AsNoTracking()
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Code)
                .ToListAsync();
            return list.Select(MapComponent).ToList();
        }

        public async Task<PayrollComponentDefinitionDto?> GetComponentByIdAsync(Guid id)
        {
            var c = await _context.PayrollComponentDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return c == null ? null : MapComponent(c);
        }

        public async Task<PayrollComponentDefinitionDto> CreateComponentAsync(PayrollComponentWriteDto dto)
        {
            var entity = new PayrollComponentDefinition
            {
                Id = Guid.NewGuid(),
                Code = dto.Code.Trim(),
                Name = dto.Name.Trim(),
                Kind = dto.Kind,
                CalculationMode = dto.CalculationMode,
                AmountOrPercent = dto.AmountOrPercent,
                MaxAmountCap = dto.MaxAmountCap,
                MinGrossEligible = dto.MinGrossEligible,
                MaxGrossEligible = dto.MaxGrossEligible,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            _context.PayrollComponentDefinitions.Add(entity);
            await _context.SaveChangesAsync();
            return MapComponent(entity);
        }

        public async Task<PayrollComponentDefinitionDto?> UpdateComponentAsync(Guid id, PayrollComponentWriteDto dto)
        {
            var entity = await _context.PayrollComponentDefinitions.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return null;
            entity.Code = dto.Code.Trim();
            entity.Name = dto.Name.Trim();
            entity.Kind = dto.Kind;
            entity.CalculationMode = dto.CalculationMode;
            entity.AmountOrPercent = dto.AmountOrPercent;
            entity.MaxAmountCap = dto.MaxAmountCap;
            entity.MinGrossEligible = dto.MinGrossEligible;
            entity.MaxGrossEligible = dto.MaxGrossEligible;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return MapComponent(entity);
        }

        public async Task<bool> DeleteComponentAsync(Guid id)
        {
            var entity = await _context.PayrollComponentDefinitions.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;
            _context.PayrollComponentDefinitions.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> SeedDefaultComponentsAsync()
        {
            if (await _context.PayrollComponentDefinitions.AnyAsync()) return 0;

            var defaults = new List<PayrollComponentDefinition>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "HRA",
                    Name = "House Rent Allowance",
                    Kind = PayrollComponentKind.Earning,
                    CalculationMode = PayrollCalculationMode.PercentOfProratedBasic,
                    AmountOrPercent = 40m,
                    SortOrder = 10,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "DA",
                    Name = "Dearness Allowance",
                    Kind = PayrollComponentKind.Earning,
                    CalculationMode = PayrollCalculationMode.PercentOfProratedBasic,
                    AmountOrPercent = 10m,
                    SortOrder = 20,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "PF",
                    Name = "Provident Fund (Employee)",
                    Kind = PayrollComponentKind.Deduction,
                    CalculationMode = PayrollCalculationMode.PercentOfProratedBasic,
                    AmountOrPercent = 12m,
                    MaxAmountCap = 1800m,
                    SortOrder = 100,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "ESI",
                    Name = "ESI (Employee)",
                    Kind = PayrollComponentKind.Deduction,
                    CalculationMode = PayrollCalculationMode.PercentOfGrossEarnings,
                    AmountOrPercent = 0.75m,
                    MinGrossEligible = 0m,
                    MaxGrossEligible = 21000m,
                    SortOrder = 110,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "PT",
                    Name = "Professional Tax",
                    Kind = PayrollComponentKind.Deduction,
                    CalculationMode = PayrollCalculationMode.FixedAmount,
                    AmountOrPercent = 200m,
                    SortOrder = 120,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.PayrollComponentDefinitions.AddRange(defaults);
            await _context.SaveChangesAsync();
            return defaults.Count;
        }

        public async Task<IReadOnlyList<PayrollPeriodDto>> GetPeriodsAsync(Guid academicYearId)
        {
            var list = await _context.PayrollPeriods
                .AsNoTracking()
                .Where(p => p.AcademicYearId == academicYearId)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync();
            var ids = list.Select(p => p.Id).ToList();
            var counts = await _context.PayrollSlips
                .Where(s => ids.Contains(s.PayrollPeriodId))
                .GroupBy(s => s.PayrollPeriodId)
                .Select(g => new { g.Key, C = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.C);
            return list.Select(p => new PayrollPeriodDto
            {
                Id = p.Id,
                AcademicYearId = p.AcademicYearId,
                Year = p.Year,
                Month = p.Month,
                Status = p.Status,
                WorkWeekMode = p.WorkWeekMode,
                AssumeFullAttendanceIfNoRecords = p.AssumeFullAttendanceIfNoRecords,
                Notes = p.Notes,
                SlipCount = counts.GetValueOrDefault(p.Id),
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        public async Task<PayrollPeriodDto?> GetPeriodByIdAsync(Guid id)
        {
            var p = await _context.PayrollPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return null;
            var slipCount = await _context.PayrollSlips.CountAsync(s => s.PayrollPeriodId == id);
            return new PayrollPeriodDto
            {
                Id = p.Id,
                AcademicYearId = p.AcademicYearId,
                Year = p.Year,
                Month = p.Month,
                Status = p.Status,
                WorkWeekMode = p.WorkWeekMode,
                AssumeFullAttendanceIfNoRecords = p.AssumeFullAttendanceIfNoRecords,
                Notes = p.Notes,
                SlipCount = slipCount,
                CreatedAt = p.CreatedAt
            };
        }

        public async Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriodCreateDto dto, Guid? createdByUserId)
        {
            var exists = await _context.PayrollPeriods.AnyAsync(p =>
                p.AcademicYearId == dto.AcademicYearId && p.Year == dto.Year && p.Month == dto.Month);
            if (exists)
                throw new InvalidOperationException("A payroll period already exists for this academic year and month.");

            var entity = new PayrollPeriod
            {
                Id = Guid.NewGuid(),
                AcademicYearId = dto.AcademicYearId,
                Year = dto.Year,
                Month = dto.Month,
                Status = "Draft",
                WorkWeekMode = string.IsNullOrWhiteSpace(dto.WorkWeekMode) ? "SchoolCalendar" : dto.WorkWeekMode.Trim(),
                AssumeFullAttendanceIfNoRecords = dto.AssumeFullAttendanceIfNoRecords,
                Notes = dto.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = createdByUserId
            };
            _context.PayrollPeriods.Add(entity);
            await _context.SaveChangesAsync();
            return (await GetPeriodByIdAsync(entity.Id))!;
        }

        public async Task<PayrollRecalculateResultDto> RecalculatePeriodAsync(Guid periodId)
        {
            var period = await _context.PayrollPeriods.IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == periodId)
                ?? throw new InvalidOperationException("Payroll period not found.");
            if (period.Status is "Paid" or "Locked")
                throw new InvalidOperationException("Cannot recalculate a paid or locked period.");

            await SeedDefaultComponentsAsync();
            var components = await _context.PayrollComponentDefinitions
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Code)
                .ToListAsync();
            var earningsDefs = components.Where(c => c.Kind == PayrollComponentKind.Earning).ToList();
            var deductionDefs = components.Where(c => c.Kind == PayrollComponentKind.Deduction).ToList();

            var monthStart = new DateTime(period.Year, period.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var holidays = await _context.Holidays
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(h => h.AcademicYearId == period.AcademicYearId && h.IsActive &&
                            h.StartDate <= monthEnd && h.EndDate >= monthStart)
                .ToListAsync();
            var holidayDates = BuildHolidayDates(holidays, monthStart, monthEnd);
            var (workingDates, calendarTrace) = await GetWorkingDatesWithCalendarAsync(
                period.AcademicYearId, period.Year, period.Month, period.WorkWeekMode, holidayDates, monthStart, monthEnd);
            var workingDays = workingDates.Count;

            var staffList = await _context.Staff
                .AsNoTracking()
                .Include(s => s.User)
                .Where(s => s.IsActive)
                .ToListAsync();

            var existingSlips = await _context.PayrollSlips.IgnoreQueryFilters()
                .Where(s => s.PayrollPeriodId == periodId).ToListAsync();
            if (existingSlips.Count > 0)
                _context.PayrollSlips.RemoveRange(existingSlips);

            decimal totalNet = 0;

            foreach (var staff in staffList)
            {
                var slip = await BuildSlipForStaffAsync(
                    period, staff, workingDates, workingDays, earningsDefs, deductionDefs, calendarTrace);
                totalNet += slip.NetPay;
                _context.PayrollSlips.Add(slip);
            }

            period.Status = "Calculated";
            period.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var count = await _context.PayrollSlips.IgnoreQueryFilters()
                .CountAsync(s => s.PayrollPeriodId == periodId);
            return new PayrollRecalculateResultDto
            {
                PeriodId = periodId,
                Status = period.Status,
                SlipCount = count,
                TotalNetPay = totalNet
            };
        }

        private async Task<PayrollSlip> BuildSlipForStaffAsync(
            PayrollPeriod period,
            Models.Staff staff,
            List<DateTime> workingDates,
            int workingDays,
            List<PayrollComponentDefinition> earningsDefs,
            List<PayrollComponentDefinition> deductionDefs,
            string calendarTrace)
        {
            var monthStart = new DateTime(period.Year, period.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var trace = new StringBuilder();
            trace.AppendLine(calendarTrace);

            var hasAnyAttendance = await _context.StaffAttendances.AnyAsync(a =>
                a.StaffId == staff.Id && a.AcademicYearId == period.AcademicYearId &&
                a.Date >= monthStart && a.Date <= monthEnd);

            int presentOnly = 0;
            int leaveOnly = 0;
            int both = 0;

            if (!hasAnyAttendance && period.AssumeFullAttendanceIfNoRecords)
            {
                trace.AppendLine("No attendance records: assumed full attendance for the month.");
                presentOnly = workingDays;
            }
            else
            {
                var workingSet = workingDates.ToHashSet();
                var presentRows = await _context.StaffAttendances.AsNoTracking()
                    .Where(a => a.StaffId == staff.Id && a.AcademicYearId == period.AcademicYearId &&
                                a.Date >= monthStart && a.Date <= monthEnd && a.IsPresent)
                    .Select(a => a.Date)
                    .ToListAsync();
                var presentOnWorking = presentRows
                    .Select(d => d.Date.Date)
                    .Where(d => workingSet.Contains(d))
                    .ToHashSet();

                var leaveRows = await _context.StaffLeaves.IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(l => l.StaffId == staff.Id && l.AcademicYearId == period.AcademicYearId &&
                                l.Status == "Approved" &&
                                l.StartDate <= monthEnd && l.EndDate >= monthStart)
                    .Select(l => new { l.StartDate, l.EndDate, l.LeaveType })
                    .ToListAsync();

                var paidLeaveOnWorking = new HashSet<DateTime>();
                foreach (var lv in leaveRows)
                {
                    if (IsUnpaidLeaveType(lv.LeaveType)) continue;
                    var s = lv.StartDate.Date < monthStart ? monthStart : lv.StartDate.Date;
                    var e = lv.EndDate.Date > monthEnd ? monthEnd : lv.EndDate.Date;
                    for (var d = s; d <= e; d = d.AddDays(1))
                    {
                        if (workingSet.Contains(d))
                            paidLeaveOnWorking.Add(d);
                    }
                }

                foreach (var d in workingDates)
                {
                    var leave = paidLeaveOnWorking.Contains(d);
                    var present = presentOnWorking.Contains(d);
                    if (leave && present) both++;
                    else if (leave) leaveOnly++;
                    else if (present) presentOnly++;
                }
            }

            int paidDays = presentOnly + leaveOnly + both;
            int lopDays = Math.Max(0, workingDays - paidDays);
            decimal proration = workingDays > 0 ? paidDays / (decimal)workingDays : 1m;
            trace.AppendLine(
                $"Paid days from live data: working days={workingDays}, paid={paidDays} (present={presentOnly}, paid leave={leaveOnly}, both={both}), LOP={lopDays}, proration={proration:P2}. Unpaid / LWP leave is not counted as paid leave.");

            var monthlyBasic = staff.Salary;
            if (monthlyBasic < 0) monthlyBasic = 0;
            var proratedBasic = Math.Round(monthlyBasic * proration, 2, MidpointRounding.AwayFromZero);

            var earningLines = new List<PayrollLineItemDto>
            {
                new() { Code = "BASIC", Name = "Basic Salary (prorated)", Amount = proratedBasic }
            };
            decimal gross = proratedBasic;

            foreach (var def in earningsDefs)
            {
                var runningForPercentGross = gross;
                var amt = ComputeComponentAmount(def, proratedBasic, runningForPercentGross, proration);
                if (amt <= 0) continue;
                earningLines.Add(new PayrollLineItemDto { Code = def.Code, Name = def.Name, Amount = amt });
                gross += amt;
            }

            gross = Math.Round(gross, 2, MidpointRounding.AwayFromZero);
            trace.AppendLine($"Gross earnings={gross}.");

            var deductionLines = new List<PayrollLineItemDto>();
            decimal dedTotal = 0;
            foreach (var def in deductionDefs)
            {
                var amt = ComputeDeductionAmount(def, proratedBasic, gross, proration);
                if (amt <= 0) continue;
                amt = Math.Round(amt, 2, MidpointRounding.AwayFromZero);
                deductionLines.Add(new PayrollLineItemDto { Code = def.Code, Name = def.Name, Amount = amt });
                dedTotal += amt;
                trace.AppendLine($"Deduction {def.Code}={amt}.");
            }

            dedTotal = Math.Round(dedTotal, 2, MidpointRounding.AwayFromZero);
            var net = Math.Round(gross - dedTotal, 2, MidpointRounding.AwayFromZero);
            trace.AppendLine($"Net pay={net}.");

            return new PayrollSlip
            {
                Id = Guid.NewGuid(),
                PayrollPeriodId = period.Id,
                StaffId = staff.Id,
                AcademicYearId = period.AcademicYearId,
                WorkingDays = workingDays,
                PresentDays = presentOnly + both,
                PaidLeaveDays = leaveOnly + both,
                LopDays = lopDays,
                ProrationFactor = proration,
                MonthlyBasicFromProfile = monthlyBasic,
                GrossTotal = gross,
                DeductionTotal = dedTotal,
                NetPay = net,
                EarningsJson = JsonSerializer.Serialize(earningLines, JsonOpts),
                DeductionsJson = JsonSerializer.Serialize(deductionLines, JsonOpts),
                CalculationTrace = trace.ToString(),
                CreatedAt = DateTime.UtcNow
            };
        }

        private static decimal ComputeComponentAmount(
            PayrollComponentDefinition def,
            decimal proratedBasic,
            decimal grossBeforeThis,
            decimal proration)
        {
            return def.CalculationMode switch
            {
                PayrollCalculationMode.FixedAmount =>
                    Math.Round(def.AmountOrPercent * proration, 2, MidpointRounding.AwayFromZero),
                PayrollCalculationMode.PercentOfProratedBasic =>
                    Math.Round(proratedBasic * (def.AmountOrPercent / 100m), 2, MidpointRounding.AwayFromZero),
                PayrollCalculationMode.PercentOfGrossEarnings =>
                    Math.Round(grossBeforeThis * (def.AmountOrPercent / 100m), 2, MidpointRounding.AwayFromZero),
                _ => 0
            };
        }

        private static decimal ComputeDeductionAmount(
            PayrollComponentDefinition def,
            decimal proratedBasic,
            decimal grossEarnings,
            decimal proration)
        {
            if (def.MinGrossEligible.HasValue && grossEarnings < def.MinGrossEligible.Value) return 0;
            if (def.MaxGrossEligible.HasValue && grossEarnings > def.MaxGrossEligible.Value) return 0;

            decimal raw = def.CalculationMode switch
            {
                PayrollCalculationMode.FixedAmount =>
                    Math.Round(def.AmountOrPercent * proration, 2, MidpointRounding.AwayFromZero),
                PayrollCalculationMode.PercentOfProratedBasic =>
                    proratedBasic * (def.AmountOrPercent / 100m),
                PayrollCalculationMode.PercentOfGrossEarnings =>
                    grossEarnings * (def.AmountOrPercent / 100m),
                _ => 0
            };

            if (def.MaxAmountCap.HasValue && raw > def.MaxAmountCap.Value)
                raw = def.MaxAmountCap.Value;
            return Math.Round(raw, 2, MidpointRounding.AwayFromZero);
        }

        private static bool IsUnpaidLeaveType(string? leaveType)
        {
            if (string.IsNullOrWhiteSpace(leaveType)) return false;
            return leaveType.Contains("Unpaid", StringComparison.OrdinalIgnoreCase)
                   || leaveType.Contains("Without pay", StringComparison.OrdinalIgnoreCase)
                   || leaveType.Contains("LWP", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<(List<DateTime> Dates, string Trace)> GetWorkingDatesWithCalendarAsync(
            Guid academicYearId,
            int year,
            int month,
            string workWeekMode,
            HashSet<DateTime> holidayDates,
            DateTime monthStart,
            DateTime monthEnd)
        {
            if (!IsSchoolCalendarMode(workWeekMode))
            {
                var dates = GetWorkingDatesInMonth(year, month, workWeekMode, holidayDates);
                var trace = $"Fixed week ({workWeekMode}): holiday dates in month={holidayDates.Count}; working days={dates.Count}.";
                return (dates, trace);
            }

            var weekOffs = await _context.WeekOffs.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(w => w.AcademicYearId == academicYearId && w.IsActive && w.IsSchoolWide
                            && w.EffectiveFrom <= monthEnd
                            && (w.EffectiveTo == null || w.EffectiveTo >= monthStart))
                .ToListAsync();

            if (weekOffs.Count == 0)
            {
                var fallback = GetWorkingDatesInMonth(year, month, "FiveDay", holidayDates);
                var trace =
                    $"School calendar: no active school-wide week-offs overlapping this month; using Mon–Fri + holidays. Holiday dates={holidayDates.Count}; working days={fallback.Count}.";
                return (fallback, trace);
            }

            var list = new List<DateTime>();
            for (var d = monthStart; d <= monthEnd; d = d.AddDays(1))
            {
                if (holidayDates.Contains(d.Date)) continue;
                if (IsDateCoveredByWeekOff(d.Date, weekOffs)) continue;
                list.Add(d.Date);
            }

            var trace2 =
                $"School calendar: holiday dates={holidayDates.Count}, week-off rules (active)={weekOffs.Count}, working days={list.Count}.";
            return (list, trace2);
        }

        private static bool IsSchoolCalendarMode(string? mode) =>
            string.Equals(mode, "SchoolCalendar", StringComparison.OrdinalIgnoreCase);

        private static bool IsDateCoveredByWeekOff(DateTime date, List<WeekOff> weekOffs)
        {
            var dow = (int)date.DayOfWeek;
            foreach (var w in weekOffs)
            {
                if (w.DayOfWeek != dow) continue;
                if (w.EffectiveFrom.Date > date.Date) continue;
                if (w.EffectiveTo.HasValue && w.EffectiveTo.Value.Date < date.Date) continue;
                return true;
            }
            return false;
        }

        private static HashSet<DateTime> BuildHolidayDates(IEnumerable<Holiday> holidays, DateTime monthStart, DateTime monthEnd)
        {
            var set = new HashSet<DateTime>();
            foreach (var h in holidays)
            {
                var s = h.StartDate.Date < monthStart ? monthStart : h.StartDate.Date;
                var e = h.EndDate.Date > monthEnd ? monthEnd : h.EndDate.Date;
                if (s > e) continue;
                for (var d = s; d <= e; d = d.AddDays(1))
                    set.Add(d);
            }
            return set;
        }

        private static List<DateTime> GetWorkingDatesInMonth(int year, int month, string workWeekMode, HashSet<DateTime> holidayDates)
        {
            var list = new List<DateTime>();
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            var six = string.Equals(workWeekMode, "SixDay", StringComparison.OrdinalIgnoreCase);
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                if (d.DayOfWeek == DayOfWeek.Sunday) continue;
                if (!six && d.DayOfWeek == DayOfWeek.Saturday) continue;
                if (holidayDates.Contains(d.Date)) continue;
                list.Add(d.Date);
            }
            return list;
        }

        public async Task<bool> FinalizePeriodAsync(Guid periodId)
        {
            var p = await _context.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == periodId);
            if (p == null) return false;
            if (p.Status != "Calculated") return false;
            p.Status = "Finalized";
            p.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkPeriodPaidAsync(Guid periodId)
        {
            var p = await _context.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == periodId);
            if (p == null) return false;
            if (p.Status != "Finalized") return false;
            p.Status = "Paid";
            p.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LockPeriodAsync(Guid periodId)
        {
            var p = await _context.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == periodId);
            if (p == null) return false;
            if (p.Status != "Paid") return false;
            p.Status = "Locked";
            p.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReopenPeriodAsync(Guid periodId)
        {
            var p = await _context.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == periodId);
            if (p == null) return false;
            if (p.Status is not ("Finalized" or "Paid")) return false;
            p.Status = "Calculated";
            p.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<PayrollSlipDto>> GetSlipsForPeriodAsync(Guid periodId)
        {
            var slips = await _context.PayrollSlips
                .AsNoTracking()
                .Include(s => s.Staff)!.ThenInclude(st => st!.User)
                .Where(s => s.PayrollPeriodId == periodId)
                .OrderBy(s => s.Staff!.User!.LastName)
                .ThenBy(s => s.Staff!.User!.FirstName)
                .ToListAsync();
            return slips.Select(MapSlip).ToList();
        }

        public async Task<PayrollSlipDto?> GetSlipByIdAsync(Guid slipId)
        {
            var s = await _context.PayrollSlips
                .AsNoTracking()
                .Include(x => x.Staff)!.ThenInclude(st => st!.User)
                .FirstOrDefaultAsync(x => x.Id == slipId);
            return s == null ? null : MapSlip(s);
        }

        private static PayrollComponentDefinitionDto MapComponent(PayrollComponentDefinition c) => new()
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            Kind = c.Kind,
            CalculationMode = c.CalculationMode,
            AmountOrPercent = c.AmountOrPercent,
            MaxAmountCap = c.MaxAmountCap,
            MinGrossEligible = c.MinGrossEligible,
            MaxGrossEligible = c.MaxGrossEligible,
            SortOrder = c.SortOrder,
            IsActive = c.IsActive
        };

        private static PayrollSlipDto MapSlip(PayrollSlip s)
        {
            var earnings = JsonSerializer.Deserialize<List<PayrollLineItemDto>>(s.EarningsJson, JsonOpts) ?? new();
            var deductions = JsonSerializer.Deserialize<List<PayrollLineItemDto>>(s.DeductionsJson, JsonOpts) ?? new();
            var staff = s.Staff;
            var user = staff?.User;
            var name = user == null ? null : $"{user.FirstName} {user.LastName}".Trim();
            return new PayrollSlipDto
            {
                Id = s.Id,
                PayrollPeriodId = s.PayrollPeriodId,
                StaffId = s.StaffId,
                EmployeeName = name,
                EmployeeId = staff?.EmployeeId,
                WorkingDays = s.WorkingDays,
                PresentDays = s.PresentDays,
                PaidLeaveDays = s.PaidLeaveDays,
                LopDays = s.LopDays,
                ProrationFactor = s.ProrationFactor,
                MonthlyBasicFromProfile = s.MonthlyBasicFromProfile,
                GrossTotal = s.GrossTotal,
                DeductionTotal = s.DeductionTotal,
                NetPay = s.NetPay,
                Earnings = earnings,
                Deductions = deductions,
                CalculationTrace = s.CalculationTrace
            };
        }
    }
}
