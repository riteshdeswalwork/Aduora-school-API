using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class CoCurricularParticipation
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }
        public Guid CoCurricularActivityId { get; set; }
        public Guid? StudentId { get; set; }

        [StringLength(100)]
        public string? Role { get; set; }

        [StringLength(200)]
        public string? Outcome { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public int? PointsAwarded { get; set; }

        public DateTime? AchievementDate { get; set; }

        public bool CertificateIssued { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
