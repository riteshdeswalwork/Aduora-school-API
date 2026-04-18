using System;

namespace sms.api.DTOs
{
    public class GenerateMonthlyDemandsRequest
    {
        /// <summary>Academic year to scope enrollments and fee structures.</summary>
        public Guid AcademicYearId { get; set; }

        /// <summary>Calendar year (e.g. 2025). Defaults to UTC previous month when omitted with Month.</summary>
        public int? Year { get; set; }

        /// <summary>Calendar month 1–12. Defaults with Year.</summary>
        public int? Month { get; set; }
    }

    public class GenerateMonthlyDemandsResultDto
    {
        public int Created { get; set; }
        public int SkippedExisting { get; set; }
        public int TargetYear { get; set; }
        public int TargetMonth { get; set; }
        public Guid AcademicYearId { get; set; }
        public string? Message { get; set; }
    }

    public class GenerateInstallmentDemandsRequest
    {
        public Guid AcademicYearId { get; set; }
        public Guid FeeStructureId { get; set; }
    }
}
