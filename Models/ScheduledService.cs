using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class ScheduledService
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        public Guid? AssignedToStaffId { get; set; }

        public DateTime ScheduledFor { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Frequency { get; set; } = "Once";

        [StringLength(30)]
        public string Status { get; set; } = "Scheduled";

        [StringLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
