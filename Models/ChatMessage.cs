using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid FromUserId { get; set; }
        [Required]
        public Guid ToUserId { get; set; }
        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;
        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        [ForeignKey(nameof(FromUserId))]
        public virtual User? FromUser { get; set; }
        [ForeignKey(nameof(ToUserId))]
        public virtual User? ToUser { get; set; }
    }
}
