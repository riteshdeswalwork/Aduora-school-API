using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class InAppNotification
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Body { get; set; } = string.Empty;

        /// <summary>e.g. leave.approved, fee.collected, system.overload</summary>
        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EntityType { get; set; }

        public Guid? EntityId { get; set; }

        [StringLength(50)]
        public string Severity { get; set; } = "info"; // info, success, warning, error

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(4000)]
        public string? DataJson { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
