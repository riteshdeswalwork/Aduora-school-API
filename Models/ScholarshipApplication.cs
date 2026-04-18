using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class ScholarshipApplication
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }
        public Guid ScholarshipId { get; set; }
        public Guid? StudentId { get; set; }

        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;

        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        [StringLength(2000)]
        public string? Remarks { get; set; }

        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        /// <summary>Amount actually disbursed to student (when status is Disbursed)</summary>
        public decimal? DisbursedAmount { get; set; }
        /// <summary>Date when amount was disbursed</summary>
        public DateTime? DisbursedAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
