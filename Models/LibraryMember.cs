using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class LibraryMember
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; } // Links to User (Student/Staff)

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        [StringLength(50)]
        public string MembershipNumber { get; set; } = string.Empty; // Unique library card number

        [Required]
        [StringLength(20)]
        public string MemberType { get; set; } = string.Empty; // Student, Staff, Faculty, Guest

        public DateTime MembershipStartDate { get; set; } = DateTime.UtcNow;
        public DateTime? MembershipEndDate { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsSuspended { get; set; } = false;

        [StringLength(500)]
        public string? SuspensionReason { get; set; }

        public DateTime? SuspensionStartDate { get; set; }
        public DateTime? SuspensionEndDate { get; set; }

        // Membership Limits
        public int MaxBooksAllowed { get; set; } = 3; // Maximum books that can be issued
        public int MaxIssueDays { get; set; } = 14; // Default issue period
        public int MaxRenewalCount { get; set; } = 2; // Maximum renewals per book

        // Current Status
        public int CurrentBooksIssued { get; set; } = 0;
        public int TotalBooksIssued { get; set; } = 0; // Lifetime count

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalFineAmount { get; set; } = 0; // Total fine accumulated

        [Column(TypeName = "decimal(10,2)")]
        public decimal PaidFineAmount { get; set; } = 0; // Total fine paid

        [Column(TypeName = "decimal(10,2)")]
        public decimal PendingFineAmount { get; set; } = 0; // Outstanding fine

        // Membership Details
        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? EmergencyContact { get; set; }

        [StringLength(20)]
        public string? EmergencyPhone { get; set; }

        // Preferences
        [StringLength(500)]
        public string? PreferredCategories { get; set; } // Comma-separated category IDs

        [StringLength(500)]
        public string? PreferredAuthors { get; set; } // Comma-separated author names

        public bool EmailNotifications { get; set; } = true;
        public bool SMSNotifications { get; set; } = true;

        // Security
        [StringLength(100)]
        public string? SecurityDeposit { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DepositAmount { get; set; }

        public DateTime? DepositDate { get; set; }

        [StringLength(100)]
        public string? DepositRefundStatus { get; set; }

        // Photo and Documents
        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        [StringLength(500)]
        public string? IdProofUrl { get; set; }

        [StringLength(100)]
        public string? IdProofType { get; set; } // Aadhaar, Passport, etc.

        [StringLength(100)]
        public string? IdProofNumber { get; set; }

        // Audit fields
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        public virtual ICollection<BookIssue> BookIssues { get; set; } = new List<BookIssue>();
        public virtual ICollection<BookReservation> BookReservations { get; set; } = new List<BookReservation>();
        public virtual ICollection<LibraryFine> LibraryFines { get; set; } = new List<LibraryFine>();

        // Computed properties
        public bool CanIssueBooks => IsActive && !IsSuspended && CurrentBooksIssued < MaxBooksAllowed && PendingFineAmount <= 0;
        public bool HasOverdueBooks => BookIssues?.Any(bi => bi.IsActive && !bi.IsReturned && bi.DueDate < DateTime.Now) ?? false;
        public int OverdueBooksCount => BookIssues?.Count(bi => bi.IsActive && !bi.IsReturned && bi.DueDate < DateTime.Now) ?? 0;
        public string MembershipStatus => IsSuspended ? "Suspended" : IsActive ? "Active" : "Inactive";
        public string DisplayName => User?.FullName ?? "Unknown Member";
        public string ContactInfo => !string.IsNullOrEmpty(Phone) ? Phone : Email ?? "No contact";
        public bool IsEligibleForRenewal => IsActive && !IsSuspended && PendingFineAmount <= 0;
        public int AvailableBookSlots => Math.Max(0, MaxBooksAllowed - CurrentBooksIssued);
        public DateTime? MembershipExpiryDate => MembershipEndDate;
        public bool IsMembershipExpired => MembershipEndDate.HasValue && MembershipEndDate.Value < DateTime.Now;
        public bool IsMembershipExpiringSoon => MembershipEndDate.HasValue && 
            MembershipEndDate.Value <= DateTime.Now.AddDays(30) && MembershipEndDate.Value > DateTime.Now;
        public string MembershipDuration => MembershipStartDate.ToString("MMM yyyy") + " - " + 
            (MembershipEndDate?.ToString("MMM yyyy") ?? "Ongoing");
        public decimal FineBalance => PendingFineAmount;
        public bool HasFineBalance => PendingFineAmount > 0;
        public string MemberTypeDisplay => MemberType switch
        {
            "Student" => "Student Member",
            "Staff" => "Staff Member",
            "Faculty" => "Faculty Member",
            "Guest" => "Guest Member",
            _ => "Member"
        };
    }
}
