using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class BookReservation
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
        public string ReservationNumber { get; set; } = string.Empty; // Unique reservation number

        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; } // When reservation expires

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Fulfilled, Expired, Cancelled

        public int Priority { get; set; } = 1; // Queue position (1 = first in queue)

        // Notification Information
        public DateTime? NotificationSentDate { get; set; }
        public bool IsNotificationSent { get; set; } = false;

        [StringLength(200)]
        public string? NotificationMethod { get; set; } // Email, SMS, Both

        // Fulfillment Information
        public DateTime? FulfilledDate { get; set; }
        public Guid? FulfilledByUserId { get; set; }
        public Guid? BookIssueId { get; set; } // Link to the actual issue when fulfilled

        [StringLength(500)]
        public string? FulfillmentNotes { get; set; }

        // Cancellation Information
        public DateTime? CancelledDate { get; set; }
        public Guid? CancelledByUserId { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        [StringLength(20)]
        public string? CancellationType { get; set; } // Member, Staff, System

        // Reservation Details
        [StringLength(1000)]
        public string? ReservationNotes { get; set; }

        [StringLength(50)]
        public string? RequestedEdition { get; set; }

        [StringLength(100)]
        public string? SpecialRequirements { get; set; }

        // Auto-extension settings
        public bool AutoExtend { get; set; } = false;
        public int ExtensionCount { get; set; } = 0;
        public int MaxExtensions { get; set; } = 2;

        // Pickup Information
        public DateTime? PickupDeadline { get; set; } // Deadline to pick up when available
        public bool IsPickupReminderSent { get; set; } = false;
        public DateTime? PickupReminderSentDate { get; set; }

        // Digital Reservation (for eBooks)
        public bool IsDigitalReservation { get; set; } = false;
        public DateTime? DigitalAccessGrantedDate { get; set; }

        // Audit fields
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;

        [ForeignKey("LibraryMemberId")]
        public virtual LibraryMember LibraryMember { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        [ForeignKey("FulfilledByUserId")]
        public virtual User? FulfilledBy { get; set; }

        [ForeignKey("CancelledByUserId")]
        public virtual User? CancelledBy { get; set; }

        [ForeignKey("BookIssueId")]
        public virtual BookIssue? BookIssue { get; set; }

        // Computed properties
        public bool IsActive => Status == "Active";
        public bool IsExpired => Status == "Expired" || (IsActive && ExpiryDate < DateTime.Now);
        public bool IsFulfilled => Status == "Fulfilled";
        public bool IsCancelled => Status == "Cancelled";
        public int DaysUntilExpiry => IsActive ? Math.Max(0, (ExpiryDate - DateTime.Now).Days) : 0;
        public int DaysSinceReservation => (DateTime.Now - ReservationDate).Days;
        public bool IsExpiringSoon => IsActive && DaysUntilExpiry <= 3;
        public string StatusColor => Status switch
        {
            "Active" => "info",
            "Fulfilled" => "success",
            "Expired" => "warning",
            "Cancelled" => "danger",
            _ => "secondary"
        };
        public string BookTitle => Book?.Title ?? "Unknown Book";
        public string MemberName => LibraryMember?.DisplayName ?? "Unknown Member";
        public string QueuePosition => Priority == 1 ? "Next in queue" : $"Position {Priority} in queue";
        public bool CanBeFulfilled => IsActive && !IsExpired && Book?.IsAvailable == true;
        public bool CanBeCancelled => IsActive && !IsFulfilled;
        public bool CanBeExtended => IsActive && !IsExpired && ExtensionCount < MaxExtensions;
        public string ReservationDuration => $"{ReservationDate:MMM dd} - {ExpiryDate:MMM dd}";
        public bool NeedsAttention => IsExpiringSoon || (IsActive && Book?.IsAvailable == true);
        public string AttentionReason => IsExpiringSoon ? $"Expires in {DaysUntilExpiry} days" :
                                        (IsActive && Book?.IsAvailable == true) ? "Book is now available" :
                                        string.Empty;
        public bool IsReadyForPickup => IsFulfilled && BookIssue != null;
        public DateTime? EstimatedAvailabilityDate => Book?.BookIssues?
            .Where(bi => bi.IsActive && !bi.IsReturned)
            .OrderBy(bi => bi.DueDate)
            .FirstOrDefault()?.DueDate;
        public string EstimatedWaitTime => EstimatedAvailabilityDate.HasValue ? 
            $"Available by {EstimatedAvailabilityDate.Value:MMM dd}" : 
            "Availability unknown";
    }
}
