using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class ClassMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OnlineClassId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        public Guid SenderId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string MessageType { get; set; } = "Text"; // Text, Image, File, System

        [StringLength(500)]
        public string AttachmentUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string AttachmentName { get; set; } = string.Empty;

        public bool IsSystemMessage { get; set; } = false;

        public bool IsPrivate { get; set; } = false;

        public Guid? RecipientId { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? EditedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("OnlineClassId")]
        public virtual OnlineClass OnlineClass { get; set; } = null!;

        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; } = null!;

        [ForeignKey("RecipientId")]
        public virtual User? Recipient { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
    }
}
