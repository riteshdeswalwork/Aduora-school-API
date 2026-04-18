using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class AcademicPerformanceHistory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        [Required]
        public Guid SubjectId { get; set; }

        public Guid? StudentAcademicHistoryId { get; set; }

        // Term/Semester Information
        [StringLength(50)]
        public string Term { get; set; } = string.Empty; // First Term, Second Term, Annual

        [StringLength(50)]
        public string ExamType { get; set; } = string.Empty; // Unit Test, Mid Term, Final, Internal Assessment

        // Marks Information
        [Column(TypeName = "decimal(6,2)")]
        public decimal MarksObtained { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal MaxMarks { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Percentage { get; set; }

        [StringLength(10)]
        public string Grade { get; set; } = string.Empty; // A+, A, B+, B, C, D, F

        [Column(TypeName = "decimal(3,1)")]
        public decimal? GradePoint { get; set; } // For GPA calculation

        // Performance Indicators
        public bool IsPassed { get; set; } = false;

        [Column(TypeName = "decimal(6,2)")]
        public decimal PassingMarks { get; set; }

        public int? SubjectRank { get; set; } // Rank in this subject within class

        public int? TotalStudentsAppeared { get; set; }

        // Detailed Assessment (for continuous evaluation)
        [Column(TypeName = "decimal(5,2)")]
        public decimal? WrittenTestMarks { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? PracticalMarks { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ProjectMarks { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AssignmentMarks { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? VivaMarks { get; set; }

        // Teacher Assessment
        [StringLength(1000)]
        public string? TeacherRemarks { get; set; }

        [StringLength(100)]
        public string? StrengthAreas { get; set; } // Comma-separated

        [StringLength(100)]
        public string? ImprovementAreas { get; set; } // Comma-separated

        // Behavioral Assessment
        [Range(1, 5)]
        public int? ClassParticipation { get; set; } // 1-5 scale

        [Range(1, 5)]
        public int? HomeworkCompletion { get; set; } // 1-5 scale

        [Range(1, 5)]
        public int? Punctuality { get; set; } // 1-5 scale

        [Range(1, 5)]
        public int? Discipline { get; set; } // 1-5 scale

        // Exam Details
        public DateTime? ExamDate { get; set; }

        public Guid? ExamId { get; set; } // Reference to Exam table

        [StringLength(100)]
        public string? ExamName { get; set; }

        // Result Status
        [StringLength(50)]
        public string ResultStatus { get; set; } = "Published"; // Draft, Published, Revised

        public DateTime? ResultPublishedDate { get; set; }

        public Guid? ResultPublishedByUserId { get; set; }

        // Remedial/Improvement
        public bool RequiresRemedialClasses { get; set; } = false;

        [StringLength(500)]
        public string? RemedialRecommendations { get; set; }

        public bool IsReExamRequired { get; set; } = false;

        [StringLength(200)]
        public string? ReExamReason { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; } = null!;

        [ForeignKey("StudentAcademicHistoryId")]
        public virtual StudentAcademicHistory? StudentAcademicHistory { get; set; }

        [ForeignKey("ExamId")]
        public virtual Exam? Exam { get; set; }

        [ForeignKey("ResultPublishedByUserId")]
        public virtual User? ResultPublishedBy { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public virtual User? UpdatedBy { get; set; }

        // Computed properties
        public bool IsExcellent => Percentage >= 90;
        public bool IsGood => Percentage >= 75 && Percentage < 90;
        public bool IsSatisfactory => Percentage >= 60 && Percentage < 75;
        public bool NeedsImprovement => Percentage < 60;
        public string PerformanceCategory => IsExcellent ? "Excellent" : IsGood ? "Good" : IsSatisfactory ? "Satisfactory" : "Needs Improvement";
        public decimal TotalInternalMarks => (WrittenTestMarks ?? 0) + (PracticalMarks ?? 0) + (ProjectMarks ?? 0) + (AssignmentMarks ?? 0) + (VivaMarks ?? 0);
    }
}
