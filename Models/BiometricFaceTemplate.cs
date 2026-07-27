using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    /// <summary>
    /// Face embedding (128-d) for one sample slot (1–3) of Staff or Student (client-side face-api).
    /// </summary>
    public class BiometricFaceTemplate
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string PersonType { get; set; } = "Staff"; // Staff | Student

        public Guid? StaffId { get; set; }
        public Guid? StudentId { get; set; }

        /// <summary>1–3 enrollment samples per person</summary>
        [Range(1, 3)]
        public int SlotIndex { get; set; } = 1;

        /// <summary>JSON array of 128 floats (face-api descriptor)</summary>
        [Required]
        public string EmbeddingJson { get; set; } = "[]";

        [StringLength(80)]
        public string ModelVersion { get; set; } = "face-api-ssd-mobilenet-v1";

        public int Quality { get; set; }

        /// <summary>Optional small JPEG preview (data URL or base64)</summary>
        public string? ThumbnailBase64 { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(StaffId))]
        public virtual Staff? Staff { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student? Student { get; set; }
    }
}
