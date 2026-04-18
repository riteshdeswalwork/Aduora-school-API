using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class PtmFeedback
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }
        public Guid? TeacherStaffId { get; set; }
        public Guid? StudentId { get; set; }

        public DateTime FeedbackDate { get; set; } = DateTime.UtcNow;

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [StringLength(2000)]
        public string? Comments { get; set; }

        [StringLength(150)]
        public string? ParentName { get; set; }

        /// <summary>Topic or focus of the PTM (e.g. "Term 1 Progress", "Behaviour").</summary>
        [StringLength(200)]
        public string? MeetingTopic { get; set; }

        [StringLength(1000)]
        public string? SpecificStrengths { get; set; }

        [StringLength(1000)]
        public string? AreasForImprovement { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
