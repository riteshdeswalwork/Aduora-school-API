namespace sms.api.DTOs
{
    public class ExamResultDTO
    {
        public Guid Id { get; set; }
        public int MarksObtained { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public bool IsPass { get; set; }
        public bool IsAbsent { get; set; }
        public string AbsentReason { get; set; } = string.Empty;
        public Guid ExamId { get; set; }
        public Guid StudentId { get; set; }
        public Guid? EnteredByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public string ExamName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentRollNumber { get; set; } = string.Empty;
        public string EnteredByName { get; set; } = string.Empty;
        public string UpdatedByName { get; set; } = string.Empty;
        public int TotalMarks { get; set; }

        // Computed properties
        public string Status => IsAbsent ? "Absent" : IsPass ? "Pass" : "Fail";
        public string DisplayMarks => IsAbsent ? "Absent" : $"{MarksObtained}/{TotalMarks}";
    }

    public class CreateExamResultDTO
    {
        public int MarksObtained { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public bool IsAbsent { get; set; } = false;
        public string AbsentReason { get; set; } = string.Empty;
        public Guid ExamId { get; set; }
        public Guid StudentId { get; set; }
    }

    public class UpdateExamResultDTO
    {
        public int MarksObtained { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public bool IsAbsent { get; set; } = false;
        public string AbsentReason { get; set; } = string.Empty;
    }

    public class BulkExamResultDTO
    {
        public Guid ExamId { get; set; }
        public List<StudentResultDTO> StudentResults { get; set; } = new();
    }

    public class StudentResultDTO
    {
        public Guid StudentId { get; set; }
        public int MarksObtained { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public bool IsAbsent { get; set; } = false;
        public string AbsentReason { get; set; } = string.Empty;
    }

    public class ExamResultSummaryDTO
    {
        public Guid ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int StudentsAppeared { get; set; }
        public int StudentsAbsent { get; set; }
        public int StudentsPassed { get; set; }
        public int StudentsFailed { get; set; }
        public decimal PassPercentage { get; set; }
        public decimal AverageMarks { get; set; }
        public int HighestMarks { get; set; }
        public int LowestMarks { get; set; }
    }
}
