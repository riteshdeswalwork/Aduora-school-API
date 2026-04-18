using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class StaffLeave : Leave
    {
        [Required]
        public Guid StaffId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        public string LeaveType { get; set; } = "Regular"; // Regular, Sick, Emergency, etc.

        [ForeignKey("StaffId")]
        public virtual Staff Staff { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
    }
}
