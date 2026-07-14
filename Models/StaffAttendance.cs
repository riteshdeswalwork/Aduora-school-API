using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class StaffAttendance
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid StaffId { get; set; }
        [Required]
        public Guid AcademicYearId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public bool IsPresent { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Present"; // Present, Absent, Late

        public string Remarks { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        [ForeignKey(nameof(StaffId))]
        public virtual Staff? Staff { get; set; }
        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear? AcademicYear { get; set; }
    }
}
