using System.ComponentModel.DataAnnotations;
using sms.api.Models.Enums;

namespace sms.api.Models.DTOs
{
    public class PayrollComponentDefinitionDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public PayrollComponentKind Kind { get; set; }
        public PayrollCalculationMode CalculationMode { get; set; }
        public decimal AmountOrPercent { get; set; }
        public decimal? MaxAmountCap { get; set; }
        public decimal? MinGrossEligible { get; set; }
        public decimal? MaxGrossEligible { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class PayrollComponentWriteDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public PayrollComponentKind Kind { get; set; }
        public PayrollCalculationMode CalculationMode { get; set; }
        public decimal AmountOrPercent { get; set; }
        public decimal? MaxAmountCap { get; set; }
        public decimal? MinGrossEligible { get; set; }
        public decimal? MaxGrossEligible { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PayrollPeriodDto
    {
        public Guid Id { get; set; }
        public Guid AcademicYearId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Status { get; set; } = string.Empty;
        public string WorkWeekMode { get; set; } = string.Empty;
        public bool AssumeFullAttendanceIfNoRecords { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int SlipCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PayrollPeriodCreateDto
    {
        public Guid AcademicYearId { get; set; }
        [Range(2000, 2100)]
        public int Year { get; set; }
        [Range(1, 12)]
        public int Month { get; set; }
        public string WorkWeekMode { get; set; } = "SchoolCalendar";
        public bool AssumeFullAttendanceIfNoRecords { get; set; } = true;
        public string Notes { get; set; } = string.Empty;
    }

    public class PayrollLineItemDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class PayrollSlipDto
    {
        public Guid Id { get; set; }
        public Guid PayrollPeriodId { get; set; }
        public Guid StaffId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeId { get; set; }
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int PaidLeaveDays { get; set; }
        public int LopDays { get; set; }
        public decimal ProrationFactor { get; set; }
        public decimal MonthlyBasicFromProfile { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal DeductionTotal { get; set; }
        public decimal NetPay { get; set; }
        public List<PayrollLineItemDto> Earnings { get; set; } = new();
        public List<PayrollLineItemDto> Deductions { get; set; } = new();
        public string? CalculationTrace { get; set; }
    }

    public class PayrollRecalculateResultDto
    {
        public Guid PeriodId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int SlipCount { get; set; }
        public decimal TotalNetPay { get; set; }
    }
}
