namespace sms.api.DTOs
{
    public class ExamDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }
        public string ExamType { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid ClassId { get; set; }
        public Guid SubjectId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public string ClassName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;

        // Computed properties
        public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public bool IsUpcoming => ExamDate > DateTime.Now;
        public bool IsCompleted => ExamDate < DateTime.Now;
        public string Status => IsUpcoming ? "Upcoming" : IsCompleted ? "Completed" : "Ongoing";
    }

    public class CreateExamDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }
        public string ExamType { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public Guid ClassId { get; set; }
        public Guid SubjectId { get; set; }
    }

    public class UpdateExamDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }
        public string ExamType { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
