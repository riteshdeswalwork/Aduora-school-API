using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sms.api.Models.Enums;

namespace sms.api.Models
{
    /// <summary>Master earning/deduction rule (e.g. HRA %, PF % with cap, fixed PT).</summary>
    public class PayrollComponentDefinition
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(64)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public PayrollComponentKind Kind { get; set; }

        [Required]
        public PayrollCalculationMode CalculationMode { get; set; }

        /// <summary>For FixedAmount: rupees; for percent modes: percentage points (e.g. 12 = 12%).</summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal AmountOrPercent { get; set; }

        /// <summary>Optional ceiling (e.g. PF employee contribution cap).</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxAmountCap { get; set; }

        /// <summary>If set, component applies only when gross &gt;= this (e.g. ESI lower band).</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinGrossEligible { get; set; }

        /// <summary>If set, component applies only when gross &lt;= this (e.g. ESI upper cap).</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxGrossEligible { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
