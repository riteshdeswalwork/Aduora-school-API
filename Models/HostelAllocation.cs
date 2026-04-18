using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class HostelAllocation
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }
        public Guid HostelRoomId { get; set; }
        public Guid? StudentId { get; set; }

        public DateTime FromDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime? ToDate { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Active";

        public decimal? SecurityDeposit { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
