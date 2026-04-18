namespace sms.api.Models.DTOs
{
    public class AnalyticsDTO
    {
        public StudentAnalyticsDTO Students { get; set; } = new();
        public StaffAnalyticsDTO Staff { get; set; } = new();
        public AttendanceAnalyticsDTO Attendance { get; set; } = new();
        public FeeAnalyticsDTO Fees { get; set; } = new();
        public ExamAnalyticsDTO Exams { get; set; } = new();
        public ClassAnalyticsDTO Classes { get; set; } = new();
        public DepartmentAnalyticsDTO Departments { get; set; } = new();
    }

    public class DashboardAnalyticsDTO
    {
        public StudentAnalyticsDTO Students { get; set; } = new();
        public StaffAnalyticsDTO Staff { get; set; } = new();
        public AttendanceAnalyticsDTO Attendance { get; set; } = new();
        public FeeAnalyticsDTO Fees { get; set; } = new();
        public ExamAnalyticsDTO Exams { get; set; } = new();
        public ClassAnalyticsDTO Classes { get; set; } = new();
        public DepartmentAnalyticsDTO Departments { get; set; } = new();
    }

    public class StudentAnalyticsDTO
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }
        public int NewAdmissionsThisMonth { get; set; }
        public List<ClassWiseStudentCountDTO> ClassWiseDistribution { get; set; } = new();
        public List<GenderWiseCountDTO> GenderDistribution { get; set; } = new();
        public List<MonthlyAdmissionDTO> MonthlyAdmissions { get; set; } = new();
    }

    public class StaffAnalyticsDTO
    {
        public int TotalStaff { get; set; }
        public int ActiveStaff { get; set; }
        public int InactiveStaff { get; set; }
        public int NewJoiningThisMonth { get; set; }
        public List<DepartmentWiseStaffCountDTO> DepartmentWiseDistribution { get; set; } = new();
        public List<DesignationWiseCountDTO> DesignationDistribution { get; set; } = new();
        public List<SalaryRangeCountDTO> SalaryDistribution { get; set; } = new();
    }

    public class AttendanceAnalyticsDTO
    {
        public double OverallAttendancePercentage { get; set; }
        public double StudentAttendancePercentage { get; set; }
        public double StaffAttendancePercentage { get; set; }
        public List<ClassWiseAttendanceDTO> ClassWiseAttendance { get; set; } = new();
        public List<MonthlyAttendanceDTO> MonthlyAttendance { get; set; } = new();
        public List<DailyAttendanceDTO> WeeklyAttendance { get; set; } = new();
    }

    public class FeeAnalyticsDTO
    {
        public decimal TotalFeeCollected { get; set; }
        public decimal TotalFeePending { get; set; }
        public decimal TotalFeeAmount { get; set; }
        public double CollectionPercentage { get; set; }
        public List<MonthlyFeeCollectionDTO> MonthlyCollection { get; set; } = new();
        public List<ClassWiseFeeStatusDTO> ClassWiseFeeStatus { get; set; } = new();
        public List<FeeTypeCollectionDTO> FeeTypeWiseCollection { get; set; } = new();
    }

    public class ExamAnalyticsDTO
    {
        public int TotalExams { get; set; }
        public int CompletedExams { get; set; }
        public int UpcomingExams { get; set; }
        public double AverageScore { get; set; }
        public List<SubjectWisePerformanceDTO> SubjectWisePerformance { get; set; } = new();
        public List<ClassWisePerformanceDTO> ClassWisePerformance { get; set; } = new();
        public List<GradeDistributionDTO> GradeDistribution { get; set; } = new();
    }

    public class ClassAnalyticsDTO
    {
        public int TotalClasses { get; set; }
        public int ActiveClasses { get; set; }
        public double AverageStudentsPerClass { get; set; }
        public List<ClassCapacityUtilizationDTO> ClassCapacityUtilization { get; set; } = new();
    }

    public class DepartmentAnalyticsDTO
    {
        public int TotalDepartments { get; set; }
        public int ActiveDepartments { get; set; }
        public List<DepartmentWiseStaffCountDTO> DepartmentWiseStaff { get; set; } = new();
        public List<DepartmentWiseSubjectCountDTO> DepartmentWiseSubjects { get; set; } = new();
    }

    // Supporting DTOs
    public class ClassWiseStudentCountDTO
    {
        public string ClassName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class GenderWiseCountDTO
    {
        public string Gender { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class MonthlyAdmissionDTO
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DepartmentWiseStaffCountDTO
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int StaffCount { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class DesignationWiseCountDTO
    {
        public string Designation { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class SalaryRangeCountDTO
    {
        public string SalaryRange { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class ClassWiseAttendanceDTO
    {
        public string ClassName { get; set; } = string.Empty;
        public double AttendancePercentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class MonthlyAttendanceDTO
    {
        public string Month { get; set; } = string.Empty;
        public double StudentAttendance { get; set; }
        public double StaffAttendance { get; set; }
    }

    public class DailyAttendanceDTO
    {
        public string Day { get; set; } = string.Empty;
        public double AttendancePercentage { get; set; }
    }

    public class MonthlyFeeCollectionDTO
    {
        public string Month { get; set; } = string.Empty;
        public decimal Collected { get; set; }
        public decimal Pending { get; set; }
        /// <summary>Total demanded (ledger face value) for fees billed in this calendar month.</summary>
        public decimal Billed { get; set; }
    }

    public class ClassWiseFeeStatusDTO
    {
        public string ClassName { get; set; } = string.Empty;
        public decimal Collected { get; set; }
        public decimal Pending { get; set; }
        public double CollectionPercentage { get; set; }
    }

    public class FeeTypeCollectionDTO
    {
        public string FeeType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class SubjectWisePerformanceDTO
    {
        public string SubjectName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
        public int TotalStudents { get; set; }
    }

    public class ClassWisePerformanceDTO
    {
        public string ClassName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
        public int TotalStudents { get; set; }
    }

    public class GradeDistributionDTO
    {
        public string Grade { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class ClassCapacityUtilizationDTO
    {
        public string ClassName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentStudents { get; set; }
        public double UtilizationPercentage { get; set; }
    }

    public class DepartmentWiseSubjectCountDTO
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int SubjectCount { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
