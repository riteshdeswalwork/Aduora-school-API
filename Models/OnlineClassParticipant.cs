using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class OnlineClassParticipant
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OnlineClassId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Student"; // Teacher, Student

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LeftAt { get; set; }

        public bool IsMicEnabled { get; set; } = false;

        public bool IsCameraEnabled { get; set; } = false;

        public bool IsHandRaised { get; set; } = false;

        public bool IsPresent { get; set; } = true;

        [StringLength(100)]
        public string ConnectionId { get; set; } = string.Empty;

        [StringLength(500)]
        public string DeviceInfo { get; set; } = string.Empty;

        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OnlineClassId")]
        public virtual OnlineClass OnlineClass { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
    }
}
