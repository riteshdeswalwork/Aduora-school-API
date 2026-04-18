using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class Attendance
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsPresent { get; set; }

        public string Remarks { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        // Navigation properties
        public virtual Student? Student { get; set; }

        public virtual Course? Course { get; set; }

        public virtual AcademicYear? AcademicYear { get; set; }
    }
}
