using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class ClassAttendance
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string Remarks { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public Guid CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYear { get; set; }

        public virtual ICollection<ClassAttendanceRecord> AttendanceRecords { get; set; } = new List<ClassAttendanceRecord>();
    }

}
