using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class StudentAcademicHistory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        [StringLength(50)]
        public string Grade { get; set; } = string.Empty; // e.g., "Class 10", "Grade 5"

        [StringLength(20)]
        public string Section { get; set; } = string.Empty; // e.g., "A", "B"

        [StringLength(20)]
        public string RollNumber { get; set; } = string.Empty;

        // Academic Performance
        [Column(TypeName = "decimal(5,2)")]
        public decimal? OverallPercentage { get; set; }

        [StringLength(10)]
        public string? OverallGrade { get; set; } // A+, A, B+, etc.

        [Column(TypeName = "decimal(8,2)")]
        public decimal? TotalMarksObtained { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? TotalMaxMarks { get; set; }

        public int? ClassRank { get; set; }
        public int? TotalStudentsInClass { get; set; }

        // Attendance Summary
        public int? TotalWorkingDays { get; set; }
        public int? TotalPresentDays { get; set; }
        public int? TotalAbsentDays { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AttendancePercentage { get; set; }

        // Fee Information
        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalFeeAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? FeePaid { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? FeeBalance { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DiscountAmount { get; set; }

        // Promotion/Result Status
        [StringLength(20)]
        public string PromotionStatus { get; set; } = "Pending"; // Promoted, Detained, Transferred

        [StringLength(50)]
        public string? NextClass { get; set; } // Class promoted to

        public DateTime? PromotionDate { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        // Disciplinary Records
        public int? TotalWarnings { get; set; }
        public int? TotalSuspensions { get; set; }

        [StringLength(1000)]
        public string? DisciplinaryRemarks { get; set; }

        // Extra-curricular Activities
        [StringLength(1000)]
        public string? ExtracurricularActivities { get; set; } // JSON or comma-separated

        [StringLength(1000)]
        public string? AchievementsAndAwards { get; set; } // JSON or comma-separated

        // Health Records Summary
        [StringLength(500)]
        public string? HealthRemarks { get; set; }

        // Status and Audit
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        // Navigation Properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [ForeignKey("UpdatedByUserId")]
        public virtual User? UpdatedBy { get; set; }

        // Related Records
        [JsonIgnore]
        public virtual ICollection<AcademicPerformanceHistory> PerformanceHistories { get; set; } = new List<AcademicPerformanceHistory>();

        // Computed Properties
        public bool IsPromoted => PromotionStatus == "Promoted";
        public bool IsDetained => PromotionStatus == "Detained";
        public string AcademicYearDisplay => AcademicYear?.YearCode ?? string.Empty;
        public string ClassDisplay => $"{Grade} {Section}".Trim();
        public decimal FeeCompletionPercentage => TotalFeeAmount > 0 ? (FeePaid ?? 0) / TotalFeeAmount.Value * 100 : 0;
    }
}
