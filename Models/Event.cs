using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Event
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [StringLength(50)]
        public string EventType { get; set; } = "General"; // General, Academic, Sports, Cultural, Meeting

        [StringLength(50)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

        // Scope - can be for entire school or specific classes
        public bool IsSchoolWide { get; set; } = true;

        // If not school-wide, specify which classes
        public string? ClassIds { get; set; } // JSON array of class IDs

        public bool IsActive { get; set; } = true;

        [Required]
        public Guid AcademicYearId { get; set; }

        public bool RequiresAttendance { get; set; } = false;

        [StringLength(200)]
        public string? AttachmentUrl { get; set; }

        public Guid CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
    }
}
