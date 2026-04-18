using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class AcademicYear
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string YearCode { get; set; } = string.Empty; // e.g., "2023-24"

        [Required]
        [StringLength(100)]
        public string YearName { get; set; } = string.Empty; // e.g., "Academic Year 2023-2024"

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsCurrentYear { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }

        // Academic terms/semesters in this year
        public int NumberOfTerms { get; set; } = 2; // Usually 2 terms per year

        [StringLength(200)]
        public string TermNames { get; set; } = "First Term,Second Term"; // Comma separated

        // Important dates
        public DateTime? AdmissionStartDate { get; set; }
        public DateTime? AdmissionEndDate { get; set; }
        public DateTime? ExamStartDate { get; set; }
        public DateTime? ExamEndDate { get; set; }
        public DateTime? ResultDeclarationDate { get; set; }

        // Financial year mapping
        [StringLength(20)]
        public string FinancialYear { get; set; } = string.Empty; // e.g., "2023-24"

        // Audit fields
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        // Related data
        public virtual ICollection<StudentAcademicHistory> StudentAcademicHistories { get; set; } = new List<StudentAcademicHistory>();
        public virtual ICollection<ClassPromotionHistory> ClassPromotionHistories { get; set; } = new List<ClassPromotionHistory>();
        public virtual ICollection<AcademicPerformanceHistory> AcademicPerformanceHistories { get; set; } = new List<AcademicPerformanceHistory>();
        public virtual ICollection<FeeHistoryByYear> FeeHistoriesByYear { get; set; } = new List<FeeHistoryByYear>();

        // Computed properties
        public bool IsCurrentlyActive => IsActive && IsCurrentYear && DateTime.Now >= StartDate && DateTime.Now <= EndDate;
        public int DurationInDays => (EndDate - StartDate).Days;
        public string[] GetTermNames() => TermNames.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }
}
