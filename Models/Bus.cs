using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class Bus
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string BusNumber { get; set; } = string.Empty; // e.g., "BUS-001"

        [Required]
        [StringLength(100)]
        public string BusName { get; set; } = string.Empty; // e.g., "School Bus A"

        [Required]
        [StringLength(50)]
        public string RegistrationNumber { get; set; } = string.Empty; // Vehicle registration

        [Required]
        public int Capacity { get; set; } // Maximum students

        [StringLength(100)]
        public string? DriverName { get; set; }

        [StringLength(15)]
        public string? DriverPhone { get; set; }

        [StringLength(100)]
        public string? ConductorName { get; set; }

        [StringLength(15)]
        public string? ConductorPhone { get; set; }

        [StringLength(50)]
        public string? Make { get; set; } // Bus manufacturer

        [StringLength(50)]
        public string? Model { get; set; } // Bus model

        public int? Year { get; set; } // Manufacturing year

        [StringLength(20)]
        public string? FuelType { get; set; } // Diesel, CNG, Electric

        public decimal? FuelCapacity { get; set; } // Fuel tank capacity

        public decimal? Mileage { get; set; } // Km per liter

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsAvailable { get; set; } = true; // Currently available for routes

        // GPS Tracking
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }

        // Maintenance
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public decimal? MaintenanceCost { get; set; }

        // Insurance
        public DateTime? InsuranceExpiryDate { get; set; }
        public decimal? InsurancePremium { get; set; }

        // Fitness Certificate
        public DateTime? FitnessExpiryDate { get; set; }

        // Audit fields
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User CreatedBy { get; set; } = null!;
        public User? UpdatedBy { get; set; }
        public ICollection<BusRoute> BusRoutes { get; set; } = new List<BusRoute>();
        public ICollection<BusMaintenanceRecord> MaintenanceRecords { get; set; } = new List<BusMaintenanceRecord>();
    }
}
