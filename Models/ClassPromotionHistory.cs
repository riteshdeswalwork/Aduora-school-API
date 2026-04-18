using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class ClassPromotionHistory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        public Guid FromClassId { get; set; }

        public Guid? ToClassId { get; set; } // Null if student was detained or left

        [StringLength(50)]
        public string FromGrade { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ToGrade { get; set; }

        [StringLength(20)]
        public string FromSection { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ToSection { get; set; }

        [Required]
        [StringLength(50)]
        public string PromotionType { get; set; } = string.Empty; // Promoted, Detained, Transferred, Dropped

        [StringLength(100)]
        public string? PromotionCriteria { get; set; } // Merit, Grace, Supplementary

        // Academic Performance at time of promotion
        [Column(TypeName = "decimal(5,2)")]
        public decimal? FinalPercentage { get; set; }

        [StringLength(10)]
        public string? FinalGrade { get; set; }

        public int? ClassRank { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AttendancePercentage { get; set; }

        // Promotion Decision Details
        public DateTime PromotionDecisionDate { get; set; }

        public Guid DecisionMadeByUserId { get; set; }

        [StringLength(50)]
        public string DecisionMadeByRole { get; set; } = string.Empty; // Principal, Vice-Principal, Class Teacher

        [StringLength(1000)]
        public string? PromotionRemarks { get; set; }

        [StringLength(1000)]
        public string? TeacherRecommendation { get; set; }

        // Special Cases
        public bool IsGracePromotion { get; set; } = false;

        [StringLength(500)]
        public string? GracePromotionReason { get; set; }

        public bool RequiresSupplementaryExam { get; set; } = false;

        [StringLength(500)]
        public string? SupplementaryExamSubjects { get; set; } // Comma-separated subject names

        // Fee Clearance
        public bool IsFeeCleared { get; set; } = false;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PendingFeeAmount { get; set; }

        // Document Status
        public bool AreDocumentsComplete { get; set; } = true;

        [StringLength(500)]
        public string? MissingDocuments { get; set; }

        // Transfer Details (if applicable)
        [StringLength(200)]
        public string? TransferToSchool { get; set; }

        public DateTime? TransferDate { get; set; }

        [StringLength(100)]
        public string? TransferReason { get; set; }

        [StringLength(50)]
        public string? TransferCertificateNumber { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Student Student { get; set; } = null!;
        public virtual AcademicYear AcademicYear { get; set; } = null!;
        public virtual Class FromClass { get; set; } = null!;
        public virtual Class? ToClass { get; set; }
        public virtual User DecisionMadeBy { get; set; } = null!;

        // Computed properties
        public bool IsPromoted => PromotionType == "Promoted";
        public bool IsDetained => PromotionType == "Detained";
        public bool IsTransferred => PromotionType == "Transferred";
        public bool IsDropped => PromotionType == "Dropped";
        public string PromotionSummary => $"{FromGrade} → {(ToGrade ?? "N/A")} ({PromotionType})";
        public bool HasPendingRequirements => !IsFeeCleared || !AreDocumentsComplete || RequiresSupplementaryExam;
    }
}
