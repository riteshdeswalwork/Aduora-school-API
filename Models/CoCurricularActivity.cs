using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class CoCurricularActivity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        public DateTime ActivityDate { get; set; } = DateTime.UtcNow.Date;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(150)]
        public string? Organizer { get; set; }

        [StringLength(200)]
        public string? Venue { get; set; }

        public int? TotalParticipants { get; set; }

        public decimal? BudgetAllocated { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Planned, Ongoing, Completed, Cancelled

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
