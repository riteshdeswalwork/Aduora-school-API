using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class StudentLeave : Leave
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        public Guid? ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYear { get; set; }
    }
}
