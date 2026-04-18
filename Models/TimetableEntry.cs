using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class TimetableEntry
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PeriodId { get; set; }

        [ForeignKey("PeriodId")]
        public virtual Period Period { get; set; } = null!;

        [Required]
        public Guid ClassId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        public Guid? SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject? Subject { get; set; }

        public Guid? TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Staff? Teacher { get; set; }

        // For substitute teacher assignments
        public Guid? SubstituteTeacherId { get; set; }

        [ForeignKey("SubstituteTeacherId")]
        public virtual Staff? SubstituteTeacher { get; set; }

        public DateTime? SubstituteDate { get; set; } // Specific date for substitute

        /// <summary>When set, this entry applies only on this calendar date. When null, entry repeats every week on <see cref="Period"/>'s day.</summary>
        public DateTime? ScheduleDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public Guid CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Computed properties
        public Staff? EffectiveTeacher => SubstituteTeacher ?? Teacher;
        public bool HasSubstitute => SubstituteTeacher != null;
        public bool IsSubstituteActive => SubstituteDate.HasValue && SubstituteDate.Value.Date == DateTime.Today;
    }
}
