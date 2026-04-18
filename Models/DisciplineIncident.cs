using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class DisciplineIncident
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }
        public Guid? StudentId { get; set; }

        public DateTime IncidentDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? IncidentType { get; set; }

        [StringLength(100)]
        public string? IncidentLocation { get; set; }

        [StringLength(30)]
        public string? Severity { get; set; } // Low, Medium, High, Critical

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? WitnessJson { get; set; }

        [StringLength(2000)]
        public string? ActionTaken { get; set; }

        [StringLength(1000)]
        public string? ActionPlan { get; set; }

        [StringLength(1000)]
        public string? Punishment { get; set; }

        public DateTime? FollowUpDate { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Open";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
