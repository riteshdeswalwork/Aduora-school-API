using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Book
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Subtitle { get; set; }

        [Required]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty; // International Standard Book Number

        [StringLength(20)]
        public string? ISBN13 { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        public Guid PublisherId { get; set; }

        // Book Details
        [StringLength(100)]
        public string? Edition { get; set; }

        public int? PublicationYear { get; set; }

        [StringLength(100)]
        public string? Language { get; set; } = "English";

        public int? Pages { get; set; }

        [StringLength(50)]
        public string? Format { get; set; } // Hardcover, Paperback, eBook, etc.

        [StringLength(100)]
        public string? Dimensions { get; set; } // Length x Width x Height

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Weight { get; set; } // in grams

        // Content Information
        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Summary { get; set; }

        [StringLength(500)]
        public string? TableOfContents { get; set; }

        [StringLength(1000)]
        public string? Keywords { get; set; } // Comma-separated for search

        // Library Management
        [Required]
        [StringLength(50)]
        public string AccessionNumber { get; set; } = string.Empty; // Unique library identifier

        [StringLength(50)]
        public string? CallNumber { get; set; } // Dewey Decimal or other classification

        [StringLength(100)]
        public string? Location { get; set; } // Shelf location

        [StringLength(50)]
        public string? Barcode { get; set; } // For barcode scanning

        // Acquisition Information
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PurchasePrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? CurrentValue { get; set; }

        public DateTime? AcquisitionDate { get; set; }

        [StringLength(100)]
        public string? AcquisitionMethod { get; set; } // Purchase, Donation, Exchange

        [StringLength(200)]
        public string? Vendor { get; set; }

        [StringLength(100)]
        public string? InvoiceNumber { get; set; }

        // Physical Condition
        [StringLength(50)]
        public string Condition { get; set; } = "Good"; // Excellent, Good, Fair, Poor, Damaged

        [StringLength(1000)]
        public string? ConditionNotes { get; set; }

        public DateTime? LastConditionCheck { get; set; }

        // Availability and Status
        public bool IsAvailable { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public bool IsLost { get; set; } = false;

        public bool IsDamaged { get; set; } = false;

        public bool IsReserved { get; set; } = false;

        [StringLength(500)]
        public string? StatusNotes { get; set; }

        // Digital Information
        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        [StringLength(500)]
        public string? EBookUrl { get; set; }

        public bool HasEBook { get; set; } = false;

        public bool HasAudioBook { get; set; } = false;

        [StringLength(500)]
        public string? AudioBookUrl { get; set; }

        // Rating and Reviews
        [Range(1, 5)]
        public decimal? AverageRating { get; set; }

        public int ReviewCount { get; set; } = 0;

        // Usage Statistics
        public int TotalIssues { get; set; } = 0;

        public DateTime? LastIssuedDate { get; set; }

        public DateTime? LastReturnedDate { get; set; }

        public int CurrentReservations { get; set; } = 0;

        // Audit fields
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual BookCategory Category { get; set; } = null!;

        [ForeignKey("PublisherId")]
        public virtual Publisher Publisher { get; set; } = null!;

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public virtual ICollection<BookIssue> BookIssues { get; set; } = new List<BookIssue>();
        public virtual ICollection<BookReservation> BookReservations { get; set; } = new List<BookReservation>();
        public virtual ICollection<BookReview> BookReviews { get; set; } = new List<BookReview>();

        // Computed properties
        public string DisplayTitle => !string.IsNullOrEmpty(Subtitle) ? $"{Title}: {Subtitle}" : Title;
        public string PrimaryAuthor => BookAuthors?.FirstOrDefault(ba => ba.IsPrimaryAuthor)?.Author?.Name ?? 
                                      BookAuthors?.FirstOrDefault()?.Author?.Name ?? "Unknown Author";
        public string AllAuthors => string.Join(", ", BookAuthors?.OrderBy(ba => ba.AuthorOrder)
                                                                  .Select(ba => ba.Author.Name) ?? new List<string>());
        public bool IsCurrentlyIssued => BookIssues?.Any(bi => bi.IsActive && !bi.IsReturned) ?? false;
        public string StatusDisplay => IsLost ? "Lost" : 
                                      IsDamaged ? "Damaged" : 
                                      !IsActive ? "Inactive" : 
                                      !IsAvailable ? "Issued" : 
                                      IsReserved ? "Reserved" : "Available";
        public string ConditionColor => Condition switch
        {
            "Excellent" => "success",
            "Good" => "info",
            "Fair" => "warning",
            "Poor" => "danger",
            "Damaged" => "dark",
            _ => "secondary"
        };
        public bool NeedsAttention => IsLost || IsDamaged || Condition == "Poor" || Condition == "Damaged";
        public int DaysSinceLastIssue => LastIssuedDate.HasValue ? 
            (DateTime.Now - LastIssuedDate.Value).Days : int.MaxValue;
        public bool IsPopular => TotalIssues > 10; // Configurable threshold
        public string ShortDescription => Description?.Length > 200 ? 
            Description.Substring(0, 200) + "..." : Description ?? string.Empty;
    }
}
