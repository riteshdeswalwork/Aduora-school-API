using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class PayrollSlip
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PayrollPeriodId { get; set; }

        [Required]
        public Guid StaffId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int PaidLeaveDays { get; set; }
        public int LopDays { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal ProrationFactor { get; set; }

        /// <summary>Staff.Salary snapshot at calculation time (full month basic).</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyBasicFromProfile { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DeductionTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPay { get; set; }

        /// <summary>JSON array of { code, name, amount }.</summary>
        public string EarningsJson { get; set; } = "[]";

        /// <summary>JSON array of { code, name, amount }.</summary>
        public string DeductionsJson { get; set; } = "[]";

        [StringLength(4000)]
        public string? CalculationTrace { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(PayrollPeriodId))]
        public virtual PayrollPeriod? PayrollPeriod { get; set; }

        [ForeignKey(nameof(StaffId))]
        public virtual Staff? Staff { get; set; }
    }
}
