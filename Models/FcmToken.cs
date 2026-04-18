using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class FcmToken
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Token { get; set; } = string.Empty;

        [StringLength(100)]
        public string? DeviceType { get; set; } // Web, Android, iOS

        [StringLength(200)]
        public string? DeviceInfo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUsedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
