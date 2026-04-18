using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class PayrollPeriod
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Range(2000, 2100)]
        public int Year { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        /// <summary>Draft → Calculated → Finalized → Paid → Locked</summary>
        [Required]
        [StringLength(32)]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// FiveDay / SixDay = fixed pattern + holidays; SchoolCalendar = holidays + active school-wide <see cref="WeekOff"/> rules (effective dates).
        /// </summary>
        [Required]
        [StringLength(16)]
        public string WorkWeekMode { get; set; } = "SchoolCalendar";

        /// <summary>When no StaffAttendance rows exist for the month, treat as full paid days.</summary>
        public bool AssumeFullAttendanceIfNoRecords { get; set; } = true;

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedByUserId { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear? AcademicYear { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedBy { get; set; }

        public virtual ICollection<PayrollSlip> Slips { get; set; } = new List<PayrollSlip>();
    }
}
