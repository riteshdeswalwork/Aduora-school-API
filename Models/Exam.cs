using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class Exam
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ExamDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(1, 1000)]
        public int TotalMarks { get; set; }

        [Required]
        [Range(1, 1000)]
        public int PassingMarks { get; set; }

        [Required]
        [StringLength(50)]
        public string ExamType { get; set; } = string.Empty; // Unit Test, Mid Term, Final, etc.

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Term { get; set; } = string.Empty; // First Term, Second Term, etc.

        public bool IsActive { get; set; } = true;

        // Foreign keys
        [Required]
        public Guid ClassId { get; set; }

        [Required]
        public Guid SubjectId { get; set; }

        public Guid CreatedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; } = null!;

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYearNavigation { get; set; }

        [JsonIgnore]
        public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Computed properties
        public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public bool IsUpcoming => ExamDate > DateTime.Now;
        public bool IsCompleted => ExamDate < DateTime.Now;
    }
}
