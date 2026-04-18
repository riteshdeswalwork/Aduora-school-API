using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class BookReview
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
        [Range(1, 5)]
        public int Rating { get; set; } // 1-5 stars

        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? ReviewText { get; set; }

        [StringLength(500)]
        public string? Pros { get; set; } // What they liked

        [StringLength(500)]
        public string? Cons { get; set; } // What they didn't like

        [StringLength(200)]
        public string? RecommendedFor { get; set; } // Age group, interests, etc.

        // Review Status
        public bool IsApproved { get; set; } = false;
        public bool IsPublished { get; set; } = false;
        public bool IsFlagged { get; set; } = false;

        [StringLength(500)]
        public string? ModerationNotes { get; set; }

        public DateTime? ApprovedDate { get; set; }
        public Guid? ApprovedByUserId { get; set; }

        // Interaction Metrics
        public int HelpfulVotes { get; set; } = 0;
        public int NotHelpfulVotes { get; set; } = 0;
        public int TotalVotes { get; set; } = 0;

        // Reading Information
        public bool HasReadBook { get; set; } = true; // Verified if they actually read it
        public DateTime? ReadingStartDate { get; set; }
        public DateTime? ReadingEndDate { get; set; }

        [StringLength(50)]
        public string? ReadingFormat { get; set; } // Physical, eBook, Audiobook

        // Additional Details
        [StringLength(1000)]
        public string? FavoriteQuote { get; set; }

        [StringLength(500)]
        public string? SimilarBooks { get; set; } // Recommendations

        public bool WouldRecommend { get; set; } = true;

        [StringLength(200)]
        public string? ReadingLevel { get; set; } // Easy, Moderate, Challenging

        [StringLength(200)]
        public string? Genre { get; set; }

        [StringLength(200)]
        public string? Mood { get; set; } // Inspiring, Sad, Funny, etc.

        // Spoiler Management
        public bool ContainsSpoilers { get; set; } = false;

        [StringLength(500)]
        public string? SpoilerWarning { get; set; }

        // Review Source
        [StringLength(50)]
        public string ReviewSource { get; set; } = "Library"; // Library, Website, Mobile App

        [StringLength(100)]
        public string? DeviceUsed { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;

        [ForeignKey("LibraryMemberId")]
        public virtual LibraryMember LibraryMember { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey("ApprovedByUserId")]
        public virtual User? ApprovedBy { get; set; }

        // Computed properties
        public string ReviewerName => LibraryMember?.DisplayName ?? "Anonymous";
        public string BookTitle => Book?.Title ?? "Unknown Book";
        public string RatingStars => new string('★', Rating) + new string('☆', 5 - Rating);
        public decimal HelpfulnessRatio => TotalVotes > 0 ? (decimal)HelpfulVotes / TotalVotes * 100 : 0;
        public bool IsHelpful => HelpfulnessRatio >= 60; // 60% or more found it helpful
        public string ReviewStatus => IsFlagged ? "Flagged" :
                                     !IsApproved ? "Pending Approval" :
                                     IsPublished ? "Published" :
                                     "Approved";
        public string StatusColor => ReviewStatus switch
        {
            "Published" => "success",
            "Approved" => "info",
            "Pending Approval" => "warning",
            "Flagged" => "danger",
            _ => "secondary"
        };
        public int ReadingDays => ReadingStartDate.HasValue && ReadingEndDate.HasValue ?
            (ReadingEndDate.Value - ReadingStartDate.Value).Days : 0;
        public string ReadingDuration => ReadingDays > 0 ? 
            ReadingDays == 1 ? "1 day" : $"{ReadingDays} days" : 
            "Unknown";
        public bool IsRecentReview => (DateTime.Now - CreatedAt).Days <= 30;
        public string ReviewAge => (DateTime.Now - CreatedAt).Days switch
        {
            0 => "Today",
            1 => "Yesterday",
            var days when days <= 7 => $"{days} days ago",
            var days when days <= 30 => $"{days / 7} weeks ago",
            var days when days <= 365 => $"{days / 30} months ago",
            var days => $"{days / 365} years ago"
        };
        public bool CanBeEdited => (DateTime.Now - CreatedAt).Days <= 7 && !IsPublished;
        public string ShortReview => ReviewText?.Length > 150 ? 
            ReviewText.Substring(0, 150) + "..." : 
            ReviewText ?? string.Empty;
        public bool HasDetailedReview => !string.IsNullOrEmpty(ReviewText) && ReviewText.Length > 100;
        public string RecommendationText => WouldRecommend ? 
            "Recommends this book" : 
            "Does not recommend this book";
        public bool NeedsModeration => !IsApproved && !IsFlagged;
        public bool IsQualityReview => HasDetailedReview && Rating > 0 && HasReadBook;
    }
}
