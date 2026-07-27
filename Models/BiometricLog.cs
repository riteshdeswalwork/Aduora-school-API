using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class BiometricLog
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? DeviceId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? StudentId { get; set; }
        public Guid? StaffId { get; set; }

        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        [StringLength(30)]
        public string LogType { get; set; } = "IN";

        [StringLength(500)]
        public string? RawData { get; set; }

        /// <summary>Fingerprint device user id that matched.</summary>
        [StringLength(100)]
        public string? ExternalUserId { get; set; }

        /// <summary>True when this punch also wrote staff/student attendance for the day.</summary>
        public bool AttendanceApplied { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
