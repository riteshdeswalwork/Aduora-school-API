using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class StudentBehaviorIncident
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }
        public Guid? StudentId { get; set; }
        public Guid? ClassId { get; set; }

        public DateTime IncidentDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? BehaviorType { get; set; }

        [StringLength(100)]
        public string? Archetype { get; set; }

        /// <summary>0=Observation/Low, 1=Attention/Medium, 2=Concern/High, 3=Critical, 4=Positive</summary>
        public int SeverityIndex { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? InterventionPlan { get; set; }

        public bool PsychReferral { get; set; }

        [StringLength(4000)]
        public string? ParentFeedbackCycle { get; set; }

        public DateTime? FollowUpDate { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Open";

        public Guid? ReportedByUserId { get; set; }
        [StringLength(200)]
        public string? LocationOfIncident { get; set; }
        [StringLength(500)]
        public string? Witnesses { get; set; }
        [StringLength(1000)]
        public string? ActionTaken { get; set; }
        [StringLength(200)]
        public string? ConsequenceType { get; set; }
        public DateTime? ResolvedAt { get; set; }
        [StringLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }
        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }
    }
}
