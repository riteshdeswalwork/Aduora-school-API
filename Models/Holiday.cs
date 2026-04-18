using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Holiday
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(50)]
        public string HolidayType { get; set; } = "General"; // General, Religious, National, Academic

        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        public string RecurrencePattern { get; set; } = string.Empty; // Yearly, Monthly, etc.

        // Scope - can be for entire school or specific classes
        public bool IsSchoolWide { get; set; } = true;

        // If not school-wide, specify which classes
        public string? ClassIds { get; set; } // JSON array of class IDs

        public bool IsActive { get; set; } = true;

        [Required]
        public Guid AcademicYearId { get; set; }

        public Guid CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
    }
}
