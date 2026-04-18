using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class EventDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = "General";
        public string Priority { get; set; } = "Normal";
        public bool IsSchoolWide { get; set; } = true;
        public List<Guid>? ClassIds { get; set; }
        public bool IsActive { get; set; } = true;
        public bool RequiresAttendance { get; set; } = false;
        public string? AttachmentUrl { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateEventDTO
    {
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
        public string EventType { get; set; } = "General";

        [StringLength(50)]
        public string Priority { get; set; } = "Normal";

        public bool IsSchoolWide { get; set; } = true;

        public List<Guid>? ClassIds { get; set; }

        public bool IsActive { get; set; } = true;

        public bool RequiresAttendance { get; set; } = false;

        [StringLength(200)]
        public string? AttachmentUrl { get; set; }
    }

    public class UpdateEventDTO
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? EventType { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; }

        public bool? IsSchoolWide { get; set; }

        public List<Guid>? ClassIds { get; set; }

        public bool? IsActive { get; set; }

        public bool? RequiresAttendance { get; set; }

        [StringLength(200)]
        public string? AttachmentUrl { get; set; }
    }
}
