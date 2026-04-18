using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class AchievementTalent
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }
        public Guid? StudentId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(100)]
        public string? Level { get; set; }

        public DateTime AchievementDate { get; set; } = DateTime.UtcNow.Date;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(150)]
        public string? AwardedBy { get; set; }

        /// <summary>Achievement or Talent - used to separate tabs in UI.</summary>
        [StringLength(30)]
        public string? Type { get; set; }

        /// <summary>Event/competition name (e.g. Inter-School Science Olympiad).</summary>
        [StringLength(200)]
        public string? EventName { get; set; }

        /// <summary>Position/rank (e.g. 1st, 2nd, Participation, Special Mention).</summary>
        [StringLength(100)]
        public string? Position { get; set; }

        public bool CertificateIssued { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
