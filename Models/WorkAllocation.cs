using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class WorkAllocation
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? AssignedToStaffId { get; set; }
        public Guid? AssignedByUserId { get; set; }
        public Guid? ReviewerStaffId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        [Range(0, 100)]
        public int CompletionPercentage { get; set; }

        [StringLength(30)]
        public string Priority { get; set; } = "Normal";

        [StringLength(30)]
        public string Complexity { get; set; } = "Medium";

        [StringLength(30)]
        public string Status { get; set; } = "Open";

        [StringLength(1000)]
        public string? ReviewerComments { get; set; }

        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
