using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    /// <summary>
    /// Maps a fingerprint device user id (ExternalUserId) to Staff or Student.
    /// Device matches the finger; SMS resolves who it is and marks attendance.
    /// </summary>
    public class BiometricUserMap
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? DeviceId { get; set; }

        /// <summary>User id enrolled on the biometric device (e.g. ZKTeco pin).</summary>
        [Required]
        [StringLength(100)]
        public string ExternalUserId { get; set; } = string.Empty;

        [StringLength(20)]
        public string PersonType { get; set; } = "Staff"; // Staff | Student

        public Guid? StaffId { get; set; }
        public Guid? StudentId { get; set; }

        [StringLength(200)]
        public string? DisplayName { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(DeviceId))]
        public virtual BiometricDevice? Device { get; set; }

        [ForeignKey(nameof(StaffId))]
        public virtual Staff? Staff { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student? Student { get; set; }
    }
}
