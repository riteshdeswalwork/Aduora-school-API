using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    /// <summary>
    /// ISO fingerprint template for one finger (1–10) of Staff or Student (MFS100).
    /// </summary>
    public class BiometricFingerTemplate
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string PersonType { get; set; } = "Staff"; // Staff | Student

        public Guid? StaffId { get; set; }
        public Guid? StudentId { get; set; }

        /// <summary>1–10: L1..L5 then R1..R5</summary>
        [Range(1, 10)]
        public int FingerIndex { get; set; }

        [StringLength(10)]
        public string FingerCode { get; set; } = "L1";

        [Required]
        public string IsoTemplate { get; set; } = string.Empty;

        public int Quality { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(StaffId))]
        public virtual Staff? Staff { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student? Student { get; set; }
    }
}
