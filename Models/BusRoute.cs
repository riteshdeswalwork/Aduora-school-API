using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class BusRoute
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RouteName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public Guid? AcademicYearId { get; set; }
        public AcademicYear? AcademicYear { get; set; }

        [StringLength(50)]
        public string? RouteCode { get; set; }

        public Guid? BusId { get; set; }
        [ForeignKey("BusId")]
        public virtual Bus? Bus { get; set; }

        public Guid? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedBy { get; set; }

        public decimal TotalDistance { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }

        public TimeSpan? MorningStartTime { get; set; }
        public TimeSpan? MorningEndTime { get; set; }
        public TimeSpan? EveningStartTime { get; set; }
        public TimeSpan? EveningEndTime { get; set; }

        public string? RouteWaypoints { get; set; }

        public virtual ICollection<BusStop> BusStops { get; set; } = new List<BusStop>();

        [StringLength(50)]
        public string Status { get; set; } = "Active";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
