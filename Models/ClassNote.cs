using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class ClassNote
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OnlineClassId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(50)]
        public string NoteType { get; set; } = "Text"; // Text, Image, File, Link

        [StringLength(500)]
        public string AttachmentUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string AttachmentName { get; set; } = string.Empty;

        public long AttachmentSize { get; set; } = 0;

        public bool IsPublic { get; set; } = true;

        public bool IsPinned { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("OnlineClassId")]
        public virtual OnlineClass OnlineClass { get; set; } = null!;

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
    }
}
