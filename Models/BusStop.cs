using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class BusStop
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string StopName { get; set; } = string.Empty; // e.g., "City Center Stop"

        [Required]
        [StringLength(20)]
        public string StopCode { get; set; } = string.Empty; // e.g., "STOP-001"

        public Guid? RouteId { get; set; }

        public int StopOrder { get; set; } = 1; // Order in the route (1, 2, 3...)

        [StringLength(50)]
        public string Latitude { get; set; } = string.Empty; // GPS coordinates, stored as string e.g. "28.614925"

        [StringLength(50)]
        public string Longitude { get; set; } = string.Empty; // GPS coordinates, stored as string e.g. "77.207451"

        [StringLength(200)]
        public string? Address { get; set; } // Full address

        [StringLength(100)]
        public string? Landmark { get; set; } // Nearby landmark

        public decimal DistanceFromSchool { get; set; } // Distance in KM

        public TimeSpan? MorningPickupTime { get; set; } // Morning pickup time
        public TimeSpan? EveningDropTime { get; set; } // Evening drop time

        public decimal MonthlyFee { get; set; } // Monthly bus fee for this stop

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }

        // Audit fields
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public BusRoute? Route { get; set; }
        public User CreatedBy { get; set; } = null!;
        public User? UpdatedBy { get; set; }

    }


}
