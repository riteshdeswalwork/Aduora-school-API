using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class HolidayDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string HolidayType { get; set; } = "General";
        public bool IsRecurring { get; set; } = false;
        public string RecurrencePattern { get; set; } = string.Empty;
        public bool IsSchoolWide { get; set; } = true;
        public List<Guid>? ClassIds { get; set; }
        public bool IsActive { get; set; } = true;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateHolidayDTO
    {
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
        public string HolidayType { get; set; } = "General";

        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        public string RecurrencePattern { get; set; } = string.Empty;

        public bool IsSchoolWide { get; set; } = true;

        public List<Guid>? ClassIds { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateHolidayDTO
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(50)]
        public string? HolidayType { get; set; }

        public bool? IsRecurring { get; set; }

        [StringLength(50)]
        public string? RecurrencePattern { get; set; }

        public bool? IsSchoolWide { get; set; }

        public List<Guid>? ClassIds { get; set; }

        public bool? IsActive { get; set; }
    }
}
