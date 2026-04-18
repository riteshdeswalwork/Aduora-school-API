using System.ComponentModel.DataAnnotations;

namespace sms.api.DTOs
{
    public class StudentAcademicHistoryDTO
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentAdmissionNumber { get; set; } = string.Empty;
        public Guid AcademicYearId { get; set; }
        public string AcademicYearCode { get; set; } = string.Empty;
        public string AcademicYearName { get; set; } = string.Empty;
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;

        // Academic Performance
        public decimal? OverallPercentage { get; set; }
        public string? OverallGrade { get; set; }
        public decimal? TotalMarksObtained { get; set; }
        public decimal? TotalMaxMarks { get; set; }
        public int? ClassRank { get; set; }
        public int? TotalStudentsInClass { get; set; }

        // Attendance Summary
        public int? TotalWorkingDays { get; set; }
        public int? TotalPresentDays { get; set; }
        public int? TotalAbsentDays { get; set; }
        public decimal? AttendancePercentage { get; set; }

        // Fee Information
        public decimal? TotalFeeAmount { get; set; }
        public decimal? FeePaid { get; set; }
        public decimal? FeeBalance { get; set; }
        public decimal? DiscountAmount { get; set; }

        // Promotion Information
        public string PromotionStatus { get; set; } = string.Empty;
        public string? NextClass { get; set; }
        public DateTime? PromotionDate { get; set; }
        public string? Remarks { get; set; }

        // Additional Information
        public int? TotalWarnings { get; set; }
        public int? TotalSuspensions { get; set; }
        public string? DisciplinaryRemarks { get; set; }
        public string? ExtracurricularActivities { get; set; }
        public string? AchievementsAndAwards { get; set; }
        public string? HealthRemarks { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed Properties
        public bool IsPromoted { get; set; }
        public bool IsDetained { get; set; }
        public string ClassDisplay { get; set; } = string.Empty;
        public decimal FeeCompletionPercentage { get; set; }
    }

    public class CreateStudentAcademicHistoryDTO
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        [StringLength(50)]
        public string Grade { get; set; } = string.Empty;

        [StringLength(20)]
        public string Section { get; set; } = string.Empty;

        [StringLength(20)]
        public string RollNumber { get; set; } = string.Empty;

        // Academic Performance (optional - can be updated later)
        public decimal? OverallPercentage { get; set; }
        public string? OverallGrade { get; set; }
        public decimal? TotalMarksObtained { get; set; }
        public decimal? TotalMaxMarks { get; set; }
        public int? ClassRank { get; set; }
        public int? TotalStudentsInClass { get; set; }

        // Attendance Summary (optional - can be updated later)
        public int? TotalWorkingDays { get; set; }
        public int? TotalPresentDays { get; set; }
        public int? TotalAbsentDays { get; set; }
        public decimal? AttendancePercentage { get; set; }

        // Fee Information (optional - can be updated later)
        public decimal? TotalFeeAmount { get; set; }
        public decimal? FeePaid { get; set; }
        public decimal? FeeBalance { get; set; }
        public decimal? DiscountAmount { get; set; }

        // Initial Status
        public string PromotionStatus { get; set; } = "Pending";
        public string? Remarks { get; set; }
    }

    public class UpdateStudentAcademicHistoryDTO
    {
        [StringLength(50)]
        public string? Grade { get; set; }

        [StringLength(20)]
        public string? Section { get; set; }

        [StringLength(20)]
        public string? RollNumber { get; set; }

        // Academic Performance
        public decimal? OverallPercentage { get; set; }
        public string? OverallGrade { get; set; }
        public decimal? TotalMarksObtained { get; set; }
        public decimal? TotalMaxMarks { get; set; }
        public int? ClassRank { get; set; }
        public int? TotalStudentsInClass { get; set; }

        // Attendance Summary
        public int? TotalWorkingDays { get; set; }
        public int? TotalPresentDays { get; set; }
        public int? TotalAbsentDays { get; set; }
        public decimal? AttendancePercentage { get; set; }

        // Fee Information
        public decimal? TotalFeeAmount { get; set; }
        public decimal? FeePaid { get; set; }
        public decimal? FeeBalance { get; set; }
        public decimal? DiscountAmount { get; set; }

        // Promotion Information
        public string? PromotionStatus { get; set; }
        public string? NextClass { get; set; }
        public DateTime? PromotionDate { get; set; }
        public string? Remarks { get; set; }

        // Additional Information
        public int? TotalWarnings { get; set; }
        public int? TotalSuspensions { get; set; }
        public string? DisciplinaryRemarks { get; set; }
        public string? ExtracurricularActivities { get; set; }
        public string? AchievementsAndAwards { get; set; }
        public string? HealthRemarks { get; set; }

        public bool? IsActive { get; set; }
    }

    public class AcademicPerformanceUpdateDTO
    {
        public decimal? OverallPercentage { get; set; }
        public string? OverallGrade { get; set; }
        public decimal? TotalMarksObtained { get; set; }
        public decimal? TotalMaxMarks { get; set; }
        public int? ClassRank { get; set; }
        public int? TotalStudentsInClass { get; set; }
    }

    public class AttendanceSummaryDTO
    {
        public int? TotalWorkingDays { get; set; }
        public int? TotalPresentDays { get; set; }
        public int? TotalAbsentDays { get; set; }
        public decimal? AttendancePercentage { get; set; }
    }

    public class FeeSummaryDTO
    {
        public decimal? TotalFeeAmount { get; set; }
        public decimal? FeePaid { get; set; }
        public decimal? FeeBalance { get; set; }
        public decimal? DiscountAmount { get; set; }
    }

    public class StudentPromotionDTO
    {
        [Required]
        public string PromotionStatus { get; set; } = string.Empty; // Promoted, Detained, Transferred

        public string? NextClass { get; set; }
        public DateTime? PromotionDate { get; set; }
        public string? Remarks { get; set; }
    }

    public class BulkPromotionUpdateDTO
    {
        public Guid StudentId { get; set; }
        public Guid AcademicYearId { get; set; }
        public string PromotionStatus { get; set; } = string.Empty;
        public string? NextClass { get; set; }
        public string? Remarks { get; set; }
    }

    public class ClassPerformanceReportDTO
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public Guid AcademicYearId { get; set; }
        public string AcademicYearCode { get; set; } = string.Empty;
        
        public int TotalStudents { get; set; }
        public decimal AveragePercentage { get; set; }
        public decimal AverageAttendance { get; set; }
        
        public int StudentsPromoted { get; set; }
        public int StudentsDetained { get; set; }
        public int StudentsTransferred { get; set; }
        
        public decimal FeeCollectionPercentage { get; set; }
        
        public List<StudentAcademicHistoryDTO> TopPerformers { get; set; } = new();
        public List<StudentAcademicHistoryDTO> StudentsNeedingAttention { get; set; } = new();
    }

    public class StudentProgressReportDTO
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string AdmissionNumber { get; set; } = string.Empty;
        
        public List<StudentAcademicHistoryDTO> AcademicHistory { get; set; } = new();
        public List<AcademicPerformanceHistoryDTO> PerformanceHistory { get; set; } = new();
        public List<ClassPromotionHistoryDTO> PromotionHistory { get; set; } = new();
        public List<FeeHistoryByYearDTO> FeeHistory { get; set; } = new();
        
        // Overall Progress Indicators
        public decimal OverallAveragePercentage { get; set; }
        public decimal OverallAverageAttendance { get; set; }
        public int TotalYearsStudied { get; set; }
        public int YearsPromoted { get; set; }
        public int YearsDetained { get; set; }
    }

    public class AcademicYearStatisticsDTO
    {
        public Guid AcademicYearId { get; set; }
        public string AcademicYearCode { get; set; } = string.Empty;
        
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        
        // Performance Statistics
        public decimal AveragePerformancePercentage { get; set; }
        public decimal AverageAttendancePercentage { get; set; }
        
        // Promotion Statistics
        public int StudentsPromoted { get; set; }
        public int StudentsDetained { get; set; }
        public int StudentsTransferred { get; set; }
        public int StudentsDropped { get; set; }
        
        // Fee Statistics
        public decimal TotalFeeAmount { get; set; }
        public decimal TotalFeeCollected { get; set; }
        public decimal TotalFeePending { get; set; }
        public decimal FeeCollectionPercentage { get; set; }
        
        // Class-wise Statistics
        public List<ClassStatisticsDTO> ClassStatistics { get; set; } = new();
    }

    public class ClassStatisticsDTO
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public decimal AveragePerformance { get; set; }
        public decimal AverageAttendance { get; set; }
        public int StudentsPromoted { get; set; }
        public int StudentsDetained { get; set; }
        public decimal FeeCollectionPercentage { get; set; }
    }

    // Additional DTOs for related models
    public class AcademicPerformanceHistoryDTO
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid AcademicYearId { get; set; }
        public string AcademicYearCode { get; set; } = string.Empty;
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty;
        public decimal MarksObtained { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal Percentage { get; set; }
        public string Grade { get; set; } = string.Empty;
        public bool IsPassed { get; set; }
        public string? TeacherRemarks { get; set; }
        public DateTime? ExamDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ClassPromotionHistoryDTO
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid AcademicYearId { get; set; }
        public string AcademicYearCode { get; set; } = string.Empty;
        public Guid FromClassId { get; set; }
        public string FromClassName { get; set; } = string.Empty;
        public Guid? ToClassId { get; set; }
        public string? ToClassName { get; set; }
        public string PromotionType { get; set; } = string.Empty;
        public decimal? FinalPercentage { get; set; }
        public string? FinalGrade { get; set; }
        public int? ClassRank { get; set; }
        public decimal? AttendancePercentage { get; set; }
        public DateTime PromotionDecisionDate { get; set; }
        public string? PromotionRemarks { get; set; }
        public bool IsPromoted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FeeHistoryByYearDTO
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid AcademicYearId { get; set; }
        public string AcademicYearCode { get; set; } = string.Empty;
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public decimal TotalAnnualFee { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public string FeeStatus { get; set; } = string.Empty;
        public DateTime? FeeCompletionDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
