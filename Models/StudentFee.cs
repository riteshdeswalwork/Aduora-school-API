using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class StudentFee
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid FeeStructureId { get; set; }

        /// <summary>When set, this ledger line is for a component under <see cref="FeeStructure"/> (multi-line head).</summary>
        public Guid? FeeStructureLineId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingAmount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue, Partial

        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;

        /// <summary>Calendar year of billing period (e.g. monthly demand).</summary>
        public int? BillingPeriodYear { get; set; }

        /// <summary>Calendar month 1–12 of billing period.</summary>
        public int? BillingPeriodMonth { get; set; }

        /// <summary>Sequence within structure/period when multiple instalments exist.</summary>
        public int? InstallmentSequence { get; set; }

        [StringLength(40)]
        public string Source { get; set; } = "Manual"; // Manual, AutoDemand, Import

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        [ForeignKey("FeeStructureId")]
        public virtual FeeStructure? FeeStructure { get; set; }

        [ForeignKey("FeeStructureLineId")]
        public virtual FeeStructureLine? FeeStructureLine { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYear { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<FeePayment> Payments { get; set; } = new List<FeePayment>();

        [JsonIgnore]
        public virtual ICollection<FeeDiscount> Discounts { get; set; } = new List<FeeDiscount>();
    }
}
