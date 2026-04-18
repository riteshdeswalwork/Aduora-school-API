using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class ExamResult
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Range(0, 1000)]
        public int MarksObtained { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Grade { get; set; } = string.Empty; // A+, A, B+, B, C+, C, D, F

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal Percentage { get; set; }

        public bool IsPass { get; set; }

        public bool IsAbsent { get; set; } = false;

        [StringLength(100)]
        public string AbsentReason { get; set; } = string.Empty;

        // Foreign keys
        [Required]
        public Guid ExamId { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        public Guid? EnteredByUserId { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("EnteredByUserId")]
        public virtual User? EnteredBy { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public virtual User? UpdatedBy { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYear { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Computed properties
        public string Status => IsAbsent ? "Absent" : IsPass ? "Pass" : "Fail";
        public string DisplayMarks => IsAbsent ? "Absent" : $"{MarksObtained}/{Exam?.TotalMarks ?? 0}";
    }
}
