using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class WeekOffDTO
    {
        public Guid Id { get; set; }
        public Guid AcademicYearId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public bool IsSchoolWide { get; set; } = true;
        public List<Guid>? ClassIds { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateWeekOffDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 6)]
        public int DayOfWeek { get; set; }

        public bool IsSchoolWide { get; set; } = true;

        public List<Guid>? ClassIds { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        public DateTime? EffectiveTo { get; set; }
    }

    public class UpdateWeekOffDTO
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 6)]
        public int? DayOfWeek { get; set; }

        public bool? IsSchoolWide { get; set; }

        public List<Guid>? ClassIds { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }
    }
}
