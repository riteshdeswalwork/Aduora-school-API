using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class FeeHistoryByYear
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        public Guid? StudentAcademicHistoryId { get; set; }

        // Fee Structure Information
        [Column(TypeName = "decimal(10,2)")]
        public decimal AnnualTuitionFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal AdmissionFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DevelopmentFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ExaminationFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LibraryFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LaboratoryFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SportsFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TransportFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal HostelFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MiscellaneousFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAnnualFee { get; set; }

        // Payment Summary
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmountPaid { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPendingAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalDiscountAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalLateFeeCharged { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalRefundAmount { get; set; }

        // Payment Schedule Tracking
        public int TotalInstallments { get; set; }
        public int CompletedInstallments { get; set; }
        public int PendingInstallments { get; set; }
        public int OverdueInstallments { get; set; }

        // Payment Dates
        public DateTime? FirstPaymentDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? FinalDueDate { get; set; }

        // Discount Information
        [StringLength(100)]
        public string? DiscountType { get; set; } // Merit, Sibling, Staff Child, Financial Aid

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercentage { get; set; }

        [StringLength(500)]
        public string? DiscountReason { get; set; }

        public Guid? DiscountApprovedByUserId { get; set; }

        public DateTime? DiscountApprovedDate { get; set; }

        // Fee Status
        [StringLength(50)]
        public string FeeStatus { get; set; } = "Pending"; // Paid, Partially Paid, Pending, Overdue, Waived

        public DateTime? FeeCompletionDate { get; set; }

        [StringLength(500)]
        public string? FeeRemarks { get; set; }

        // Concession/Scholarship Details
        public bool HasScholarship { get; set; } = false;

        [StringLength(100)]
        public string? ScholarshipName { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ScholarshipAmount { get; set; }

        [StringLength(100)]
        public string? ScholarshipProvider { get; set; }

        // Financial Aid
        public bool HasFinancialAid { get; set; } = false;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? FinancialAidAmount { get; set; }

        [StringLength(500)]
        public string? FinancialAidReason { get; set; }

        // Payment Method Summary
        [StringLength(200)]
        public string? PreferredPaymentMethods { get; set; } // Cash, Cheque, Online, Card

        public int TotalCashPayments { get; set; }
        public int TotalChequePayments { get; set; }
        public int TotalOnlinePayments { get; set; }
        public int TotalCardPayments { get; set; }

        // Communication Records
        public int TotalRemindersSet { get; set; }
        public DateTime? LastReminderDate { get; set; }
        public int TotalNoticesSent { get; set; }
        public DateTime? LastNoticeDate { get; set; }

        // Special Cases
        public bool IsFeeConcessionCase { get; set; } = false;
        public bool IsHardshipCase { get; set; } = false;

        [StringLength(1000)]
        public string? SpecialCircumstances { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [ForeignKey("StudentAcademicHistoryId")]
        public virtual StudentAcademicHistory? StudentAcademicHistory { get; set; }

        [ForeignKey("DiscountApprovedByUserId")]
        public virtual User? DiscountApprovedBy { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public virtual User? UpdatedBy { get; set; }

        // Related collections
        [JsonIgnore]
        public virtual ICollection<FeePayment> FeePayments { get; set; } = new List<FeePayment>();

        // Computed properties
        public decimal FeeCompletionPercentage => TotalAnnualFee > 0 ? (TotalAmountPaid / TotalAnnualFee) * 100 : 0;
        public bool IsFullyPaid => TotalPendingAmount <= 0;
        public bool IsOverdue => FeeStatus == "Overdue";
        public bool HasOutstandingBalance => TotalPendingAmount > 0;
        public decimal NetPayableAmount => TotalAnnualFee - TotalDiscountAmount - (ScholarshipAmount ?? 0) - (FinancialAidAmount ?? 0);
        public string PaymentSummary => $"Paid: ₹{TotalAmountPaid:N2} / Total: ₹{TotalAnnualFee:N2}";
    }
}
