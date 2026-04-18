using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Period
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(1, 7)] // 1 = Monday, 7 = Sunday
        public int DayOfWeek { get; set; }

        [StringLength(50)]
        public string PeriodType { get; set; } = "Regular"; // Regular, Break, Lunch, Assembly

        public int OrderIndex { get; set; } = 0; // For sorting periods in order

        /// <summary>Periods are scoped by academic year; null means legacy (all years).</summary>
        public Guid? AcademicYearId { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYear { get; set; }

        // Scope - can be for entire school or specific classes
        public bool IsSchoolWide { get; set; } = true;

        // If not school-wide, specify which classes
        public string? ClassIds { get; set; } // JSON array of class IDs

        public bool IsActive { get; set; } = true;

        public Guid CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<TimetableEntry> TimetableEntries { get; set; } = new List<TimetableEntry>();

        // Computed properties
        public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    }
}
