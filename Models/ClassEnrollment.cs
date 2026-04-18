using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class ClassEnrollment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        public string AcademicYear { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public string Status { get; set; } = "Active";

        // Navigation properties
        public virtual Student? Student { get; set; }
        public virtual Class? Class { get; set; }
        public virtual AcademicYear? AcademicYearNavigation { get; set; }
    }
}
