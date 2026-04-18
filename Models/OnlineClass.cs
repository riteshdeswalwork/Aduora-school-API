using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class OnlineClass
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid ClassId { get; set; }

        [Required]
        public Guid TeacherId { get; set; }

        [Required]
        public DateTime ScheduledStartTime { get; set; }

        [Required]
        public DateTime ScheduledEndTime { get; set; }

        public DateTime? ActualStartTime { get; set; }

        public DateTime? ActualEndTime { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Live, Ended, Cancelled

        [StringLength(100)]
        public string MeetingId { get; set; } = string.Empty;

        [StringLength(500)]
        public string MeetingPassword { get; set; } = string.Empty;

        public bool IsRecordingEnabled { get; set; } = false;

        public bool IsChatEnabled { get; set; } = true;

        public bool IsNotesEnabled { get; set; } = true;

        public int MaxParticipants { get; set; } = 100;

        [Required]
        public Guid AcademicYearId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey("TeacherId")]
        public virtual Staff Teacher { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<OnlineClassParticipant> Participants { get; set; } = new List<OnlineClassParticipant>();

        [JsonIgnore]
        public virtual ICollection<ClassNote> Notes { get; set; } = new List<ClassNote>();

        [JsonIgnore]
        public virtual ICollection<ClassMessage> Messages { get; set; } = new List<ClassMessage>();
    }
}
