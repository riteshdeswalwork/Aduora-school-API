using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class ClassAttendanceRecord
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ClassAttendanceId { get; set; }
        [ForeignKey("ClassAttendanceId")]
        public virtual ClassAttendance? ClassAttendance { get; set; }

        public Guid StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Present"; // Present, Absent, Late, HalfDay

        /// <summary>Legacy DB column — keep in sync with Status for older schema.</summary>
        public bool IsPresent { get; set; } = true;

        public Guid? AcademicYearId { get; set; }
        public virtual AcademicYear? AcademicYear { get; set; }

        [StringLength(200)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
