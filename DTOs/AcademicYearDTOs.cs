using System.ComponentModel.DataAnnotations;

namespace sms.api.DTOs
{
    public class AcademicYearDTO
    {
        public Guid Id { get; set; }
        public string YearCode { get; set; } = string.Empty;
        public string YearName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrentYear { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public int NumberOfTerms { get; set; }
        public string TermNames { get; set; } = string.Empty;
        public DateTime? AdmissionStartDate { get; set; }
        public DateTime? AdmissionEndDate { get; set; }
        public DateTime? ExamStartDate { get; set; }
        public DateTime? ExamEndDate { get; set; }
        public DateTime? ResultDeclarationDate { get; set; }
        public string FinancialYear { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed properties
        public bool IsCurrentlyActive { get; set; }
        public int DurationInDays { get; set; }
        public string[] TermNamesList { get; set; } = Array.Empty<string>();
    }

    public class CreateAcademicYearDTO
    {
        [Required]
        [StringLength(20)]
        public string YearCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string YearName { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsCurrentYear { get; set; } = false;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, 4)]
        public int NumberOfTerms { get; set; } = 2;

        [StringLength(200)]
        public string TermNames { get; set; } = "First Term,Second Term";

        public DateTime? AdmissionStartDate { get; set; }
        public DateTime? AdmissionEndDate { get; set; }
        public DateTime? ExamStartDate { get; set; }
        public DateTime? ExamEndDate { get; set; }
        public DateTime? ResultDeclarationDate { get; set; }

        [StringLength(20)]
        public string FinancialYear { get; set; } = string.Empty;
    }

    public class UpdateAcademicYearDTO
    {
        [StringLength(100)]
        public string? YearName { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsCurrentYear { get; set; }
        public bool? IsActive { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, 4)]
        public int? NumberOfTerms { get; set; }

        [StringLength(200)]
        public string? TermNames { get; set; }

        public DateTime? AdmissionStartDate { get; set; }
        public DateTime? AdmissionEndDate { get; set; }
        public DateTime? ExamStartDate { get; set; }
        public DateTime? ExamEndDate { get; set; }
        public DateTime? ResultDeclarationDate { get; set; }

        [StringLength(20)]
        public string? FinancialYear { get; set; }
    }

    public class AcademicYearSummaryDTO
    {
        public Guid AcademicYearId { get; set; }
        public string YearCode { get; set; } = string.Empty;
        public string YearName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrentYear { get; set; }

        // Statistics
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalExams { get; set; }

        // Performance Summary
        public int StudentsPromoted { get; set; }
        public int StudentsDetained { get; set; }
        public int StudentsTransferred { get; set; }
        public decimal AverageAttendancePercentage { get; set; }
        public decimal AverageAcademicPerformance { get; set; }

        // Fee Summary
        public decimal TotalFeeCollected { get; set; }
        public decimal TotalFeePending { get; set; }
        public decimal FeeCollectionPercentage { get; set; }

        // Important Dates Status
        public bool IsAdmissionPeriodActive { get; set; }
        public bool IsExamPeriodActive { get; set; }
        public bool AreResultsDeclared { get; set; }
    }
}
