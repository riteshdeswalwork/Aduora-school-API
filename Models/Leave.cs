using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class Leave
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Guid? ApprovedById { get; set; }

        [ForeignKey("ApprovedById")]
        public virtual User? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }
    }
}
