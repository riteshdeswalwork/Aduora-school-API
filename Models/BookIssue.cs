using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class BookIssue
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid BookId { get; set; }

        [Required]
        public Guid LibraryMemberId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        [StringLength(50)]
        public string IssueNumber { get; set; } = string.Empty; // Unique issue transaction number

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public bool IsReturned { get; set; } = false;
        public bool IsActive { get; set; } = true;

        // Renewal Information
        public int RenewalCount { get; set; } = 0;
        public DateTime? LastRenewalDate { get; set; }
        public DateTime? NextRenewalDueDate { get; set; }

        [StringLength(500)]
        public string? RenewalNotes { get; set; }

        // Return Information
        [StringLength(50)]
        public string? ReturnCondition { get; set; } // Same as issued, Damaged, Lost, etc.

        [StringLength(1000)]
        public string? ReturnNotes { get; set; }

        public Guid? ReturnedByUserId { get; set; } // Staff who processed return

        // Fine Information
        public bool HasFine { get; set; } = false;

        [Column(TypeName = "decimal(10,2)")]
        public decimal FineAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal PaidFineAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal PendingFineAmount { get; set; } = 0;

        [StringLength(200)]
        public string? FineReason { get; set; } // Overdue, Damage, Lost, etc.

        public DateTime? FineCalculatedDate { get; set; }

        // Issue Conditions
        [StringLength(50)]
        public string IssueCondition { get; set; } = "Good"; // Condition when issued

        [StringLength(1000)]
        public string? IssueNotes { get; set; }

        [StringLength(100)]
        public string? SpecialInstructions { get; set; }

        // Digital Issue (for eBooks)
        public bool IsDigitalIssue { get; set; } = false;

        [StringLength(500)]
        public string? DigitalAccessUrl { get; set; }

        public DateTime? DigitalAccessExpiry { get; set; }

        // Notification Tracking
        public DateTime? FirstReminderSent { get; set; }
        public DateTime? SecondReminderSent { get; set; }
        public DateTime? FinalNoticeSent { get; set; }

        public int TotalRemindersSent { get; set; } = 0;

        // Lost/Damage Information
        public bool IsLost { get; set; } = false;
        public bool IsDamaged { get; set; } = false;

        public DateTime? LostReportedDate { get; set; }
        public DateTime? DamageReportedDate { get; set; }

        [StringLength(1000)]
        public string? LostDamageDescription { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ReplacementCost { get; set; }

        public bool IsReplacementPaid { get; set; } = false;

        // Audit fields
        public Guid IssuedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;

        [ForeignKey("LibraryMemberId")]
        public virtual LibraryMember LibraryMember { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey("IssuedByUserId")]
        public virtual User IssuedBy { get; set; } = null!;

        [ForeignKey("ReturnedByUserId")]
        public virtual User? ReturnedBy { get; set; }

        public virtual ICollection<LibraryFine> LibraryFines { get; set; } = new List<LibraryFine>();

        // Computed properties
        public bool IsOverdue => !IsReturned && DueDate < DateTime.Now;
        public int DaysOverdue => IsOverdue ? (DateTime.Now - DueDate).Days : 0;
        public int DaysUntilDue => IsReturned ? 0 : Math.Max(0, (DueDate - DateTime.Now).Days);
        public string Status => IsLost ? "Lost" : 
                               IsDamaged ? "Damaged" : 
                               IsReturned ? "Returned" : 
                               IsOverdue ? "Overdue" : 
                               "Issued";
        public string StatusColor => Status switch
        {
            "Returned" => "success",
            "Issued" => "info",
            "Overdue" => "warning",
            "Lost" => "danger",
            "Damaged" => "dark",
            _ => "secondary"
        };
        public bool CanBeRenewed => !IsReturned && !IsOverdue && RenewalCount < LibraryMember.MaxRenewalCount && 
                                   LibraryMember.CanIssueBooks && !IsLost && !IsDamaged;
        public DateTime? NextDueDate => CanBeRenewed ? DueDate.AddDays(LibraryMember.MaxIssueDays) : null;
        public string BookTitle => Book?.Title ?? "Unknown Book";
        public string MemberName => LibraryMember?.DisplayName ?? "Unknown Member";
        public string IssueDuration => $"{IssueDate:MMM dd} - {(IsReturned ? ReturnDate?.ToString("MMM dd") : DueDate.ToString("MMM dd"))}";
        public int TotalDaysIssued => IsReturned ? 
            (ReturnDate!.Value - IssueDate).Days : 
            (DateTime.Now - IssueDate).Days;
        public bool NeedsAttention => IsOverdue || IsLost || IsDamaged || HasFine;
        public string AttentionReason => IsLost ? "Book is lost" :
                                        IsDamaged ? "Book is damaged" :
                                        IsOverdue ? $"Overdue by {DaysOverdue} days" :
                                        HasFine ? $"Fine pending: ₹{PendingFineAmount}" :
                                        string.Empty;
        public bool IsEligibleForReturn => !IsReturned && IsActive;
        public decimal TotalCost => FineAmount + (ReplacementCost ?? 0);
        public bool HasPendingPayments => PendingFineAmount > 0 || (IsLost && !IsReplacementPaid);
    }
}
