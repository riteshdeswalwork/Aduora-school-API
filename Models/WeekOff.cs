using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class WeekOff
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 6)] // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
        public int DayOfWeek { get; set; }

        // Scope - can be for entire school or specific classes
        public bool IsSchoolWide { get; set; } = true;

        // If not school-wide, specify which classes
        public string? ClassIds { get; set; } // JSON array of class IDs

        public bool IsActive { get; set; } = true;

        [Required]
        public Guid AcademicYearId { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        public DateTime? EffectiveTo { get; set; }

        /// <summary>EveryWeek, MonthlyNth, SingleDate</summary>
        [StringLength(20)]
        public string PatternType { get; set; } = "EveryWeek";

        /// <summary>For MonthlyNth: comma-separated week numbers (1-5). 5 = last occurrence of that weekday in the month.</summary>
        [StringLength(50)]
        public string WeekOccurrences { get; set; } = string.Empty;

        public Guid CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
    }
}
