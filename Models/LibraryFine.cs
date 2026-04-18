using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class LibraryFine
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid LibraryMemberId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        public Guid? BookIssueId { get; set; } // Null for general fines not related to specific issue

        [Required]
        [StringLength(50)]
        public string FineNumber { get; set; } = string.Empty; // Unique fine reference number

        [Required]
        [StringLength(50)]
        public string FineType { get; set; } = string.Empty; // Overdue, Damage, Lost, Late Return, etc.

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal PendingAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Waived, Partial

        public DateTime FineDate { get; set; } = DateTime.UtcNow; // When fine was imposed
        public DateTime? DueDate { get; set; } // When fine payment is due
        public DateTime? PaidDate { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        // Overdue Fine Calculation
        public int? OverdueDays { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DailyFineRate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxFineAmount { get; set; }

        // Damage/Loss Fine Details
        [Column(TypeName = "decimal(10,2)")]
        public decimal? BookReplacementCost { get; set; }

        [StringLength(1000)]
        public string? DamageDescription { get; set; }

        [StringLength(500)]
        public string? DamageAssessment { get; set; }

        // Payment Information
        [StringLength(50)]
        public string? PaymentMethod { get; set; } // Cash, Card, Online, Cheque

        [StringLength(100)]
        public string? PaymentReference { get; set; } // Transaction ID, Cheque number, etc.

        public Guid? PaymentReceivedByUserId { get; set; }

        [StringLength(500)]
        public string? PaymentNotes { get; set; }

        // Waiver Information
        public bool IsWaived { get; set; } = false;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? WaivedAmount { get; set; }

        public DateTime? WaivedDate { get; set; }

        public Guid? WaivedByUserId { get; set; }

        [StringLength(500)]
        public string? WaiverReason { get; set; }

        [StringLength(100)]
        public string? WaiverApprovalLevel { get; set; } // Librarian, Principal, etc.

        // Installment Payment Support
        public bool AllowInstallments { get; set; } = false;

        public int? TotalInstallments { get; set; }

        public int? PaidInstallments { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? InstallmentAmount { get; set; }

        public DateTime? NextInstallmentDue { get; set; }

        // Notification Tracking
        public DateTime? FirstNotificationSent { get; set; }
        public DateTime? LastNotificationSent { get; set; }
        public int TotalNotificationsSent { get; set; } = 0;

        [StringLength(200)]
        public string? NotificationMethods { get; set; } // Email, SMS, Letter

        // Appeal Information
        public bool IsAppealed { get; set; } = false;
        public DateTime? AppealDate { get; set; }

        [StringLength(1000)]
        public string? AppealReason { get; set; }

        [StringLength(20)]
        public string? AppealStatus { get; set; } // Pending, Approved, Rejected

        public DateTime? AppealDecisionDate { get; set; }
        public Guid? AppealDecidedByUserId { get; set; }

        [StringLength(500)]
        public string? AppealDecisionNotes { get; set; }

        // Audit fields
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("LibraryMemberId")]
        public virtual LibraryMember LibraryMember { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey("BookIssueId")]
        public virtual BookIssue? BookIssue { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        [ForeignKey("PaymentReceivedByUserId")]
        public virtual User? PaymentReceivedBy { get; set; }

        [ForeignKey("WaivedByUserId")]
        public virtual User? WaivedBy { get; set; }

        [ForeignKey("AppealDecidedByUserId")]
        public virtual User? AppealDecidedBy { get; set; }

        // Computed properties
        public bool IsPending => Status == "Pending";
        public bool IsPaid => Status == "Paid";
        public bool IsPartiallyPaid => Status == "Partial";
        public bool IsWaivedStatus => Status == "Waived";
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && !IsPaid && !IsWaivedStatus;
        public int DaysOverdue => IsOverdue ? (DateTime.Now - DueDate!.Value).Days : 0;
        public string StatusColor => Status switch
        {
            "Paid" => "success",
            "Pending" => IsOverdue ? "danger" : "warning",
            "Partial" => "info",
            "Waived" => "secondary",
            _ => "dark"
        };
        public string MemberName => LibraryMember?.DisplayName ?? "Unknown Member";
        public string BookTitle => BookIssue?.Book?.Title ?? "General Fine";
        public decimal PaymentProgress => Amount > 0 ? (PaidAmount / Amount) * 100 : 0;
        public bool CanBeWaived => !IsPaid && !IsWaivedStatus && PendingAmount > 0;
        public bool CanAcceptPayment => !IsPaid && !IsWaivedStatus && PendingAmount > 0;
        public string FineTypeDisplay => FineType switch
        {
            "Overdue" => "Overdue Fine",
            "Damage" => "Damage Fine",
            "Lost" => "Lost Book Fine",
            "Late Return" => "Late Return Fine",
            "Processing" => "Processing Fee",
            _ => FineType
        };
        public string PaymentStatus => IsPaid ? "Fully Paid" :
                                      IsPartiallyPaid ? $"₹{PaidAmount} of ₹{Amount} paid" :
                                      IsWaivedStatus ? "Waived" :
                                      $"₹{PendingAmount} pending";
        public bool NeedsAttention => IsOverdue || (IsAppealed && AppealStatus == "Pending");
        public string AttentionReason => IsOverdue ? $"Overdue by {DaysOverdue} days" :
                                        (IsAppealed && AppealStatus == "Pending") ? "Appeal pending review" :
                                        string.Empty;
        public bool IsInstallmentDue => AllowInstallments && NextInstallmentDue.HasValue && 
                                       NextInstallmentDue.Value <= DateTime.Now && !IsPaid;
        public decimal NextInstallmentAmount => IsInstallmentDue ? (InstallmentAmount ?? 0) : 0;
        public string InstallmentStatus => AllowInstallments ? 
            $"{PaidInstallments}/{TotalInstallments} installments paid" : 
            "Single payment";
    }
}
