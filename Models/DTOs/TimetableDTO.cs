using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class TimetableEntryDTO
    {
        public Guid Id { get; set; }
        public Guid PeriodId { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public string PeriodType { get; set; } = "Regular";
        public int OrderIndex { get; set; } = 0;
        
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        
        public Guid? SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        
        public Guid? TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        
        public Guid? SubstituteTeacherId { get; set; }
        public string SubstituteTeacherName { get; set; } = string.Empty;
        public DateTime? SubstituteDate { get; set; }
        
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid AcademicYearId { get; set; }

        /// <summary>Calendar day this row applies to. Null may exist on legacy rows only.</summary>
        public DateTime? ScheduleDate { get; set; }
        
        // Computed properties
        public string EffectiveTeacherName { get; set; } = string.Empty;
        public bool HasSubstitute { get; set; } = false;
        public bool IsSubstituteActive { get; set; } = false;
    }

    public class CreateTimetableEntryDTO
    {
        [Required]
        public Guid PeriodId { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        public Guid? SubjectId { get; set; }

        public Guid? TeacherId { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        /// <summary>Required when creating; entry applies only on this calendar day.</summary>
        public DateTime? ScheduleDate { get; set; }
    }

    public class UpdateTimetableEntryDTO
    {
        public Guid? SubjectId { get; set; }

        public Guid? TeacherId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? ScheduleDate { get; set; }

        /// <summary>Not supported; API rejects updates that clear <see cref="ScheduleDate"/>.</summary>
        public bool ClearScheduleDate { get; set; }
    }

    public class CopyTimetableDayDTO
    {
        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>When null, copies for every class that has rows on the source date.</summary>
        public Guid? ClassId { get; set; }

        public bool ReplaceExistingOnTargetDate { get; set; } = true;
    }

    public class AssignSubstituteDTO
    {
        [Required]
        public Guid TimetableEntryId { get; set; }

        [Required]
        public Guid SubstituteTeacherId { get; set; }

        [Required]
        public DateTime SubstituteDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }

    public class TimetableViewDTO
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public List<TimetableEntryDTO> Entries { get; set; } = new();
        public Dictionary<int, List<TimetableEntryDTO>> DayWiseEntries { get; set; } = new();
    }

    public class WeeklyTimetableDTO
    {
        public List<TimetableViewDTO> Classes { get; set; } = new();
        public List<PeriodDTO> Periods { get; set; } = new();
        public Dictionary<int, string> DayNames { get; set; } = new();
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
    }

    public class TeacherConflictCheckDTO
    {
        public Guid TeacherId { get; set; }
        public Guid PeriodId { get; set; }
        public Guid? ExcludeTimetableEntryId { get; set; }
        public DateTime? SpecificDate { get; set; }
        /// <summary>When set, only entries in this academic year are considered for recurring conflicts.</summary>
        public Guid? AcademicYearId { get; set; }

        /// <summary>When set with <see cref="AcademicYearId"/>, conflicts include dated rows on this day and recurring rows for this weekday.</summary>
        public DateTime? ScheduleDateForConflict { get; set; }
    }
}
