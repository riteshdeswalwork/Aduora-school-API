using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class PeriodDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public string PeriodType { get; set; } = "Regular";
        public int OrderIndex { get; set; } = 0;
        public Guid? AcademicYearId { get; set; }
        public bool IsSchoolWide { get; set; } = true;
        public List<Guid>? ClassIds { get; set; }
        public bool IsActive { get; set; } = true;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Computed properties
        public int DurationMinutes { get; set; }
        public string TimeRange { get; set; } = string.Empty;
    }

    public class CreatePeriodDTO
    {
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
        [Range(1, 7)]
        public int DayOfWeek { get; set; }

        [StringLength(50)]
        public string PeriodType { get; set; } = "Regular";

        public int OrderIndex { get; set; } = 0;

        public Guid? AcademicYearId { get; set; }

        public bool IsSchoolWide { get; set; } = true;

        public List<Guid>? ClassIds { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdatePeriodDTO
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [Range(1, 7)]
        public int? DayOfWeek { get; set; }

        [StringLength(50)]
        public string? PeriodType { get; set; }

        public int? OrderIndex { get; set; }

        public Guid? AcademicYearId { get; set; }

        public bool? IsSchoolWide { get; set; }

        public List<Guid>? ClassIds { get; set; }

        public bool? IsActive { get; set; }
    }
}
