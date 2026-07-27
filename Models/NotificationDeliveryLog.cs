using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class NotificationDeliveryLog
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? NotificationId { get; set; }

        public Guid? UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string Channel { get; set; } = string.Empty; // Push, Email, Sms, InApp

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // Sent, Skipped, Failed

        [StringLength(200)]
        public string? ProviderMessageId { get; set; }

        [StringLength(1000)]
        public string? Error { get; set; }

        [StringLength(100)]
        public string? Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(NotificationId))]
        public InAppNotification? Notification { get; set; }
    }
}
