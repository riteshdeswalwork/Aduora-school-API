using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Announcement
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(50)]
        public string Priority { get; set; } = "Normal"; // High, Normal, Low

        [StringLength(50)]
        public string Category { get; set; } = "General"; // General, Academic, Sports, Events, etc.

        public DateTime PublishDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiryDate { get; set; }

        public bool IsPublished { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        [StringLength(200)]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public Guid AcademicYearId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Foreign key to User (Admin who created the announcement)
        public Guid CreatedBy { get; set; }

        // Navigation properties
        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }
    }
}
