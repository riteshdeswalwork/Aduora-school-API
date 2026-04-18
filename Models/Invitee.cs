using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class Invitee
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? EventId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? ContactNumber { get; set; }

        [StringLength(100)]
        public string? Role { get; set; }

        [StringLength(30)]
        public string RsvpStatus { get; set; } = "Pending";

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
