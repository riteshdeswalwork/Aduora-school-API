using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class BusMaintenanceRecord
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid BusId { get; set; }

        [Required]
        [StringLength(100)]
        public string MaintenanceType { get; set; } = string.Empty; // "Routine", "Repair", "Service"

        [Required]
        public DateTime MaintenanceDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public decimal Cost { get; set; }

        [StringLength(100)]
        public string? ServiceProvider { get; set; } // Garage/Service center name

        [StringLength(15)]
        public string? ServiceProviderPhone { get; set; }

        public int? Odometer { get; set; } // Vehicle reading at time of service

        public DateTime? NextServiceDate { get; set; }

        [StringLength(500)]
        public string? PartsReplaced { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public bool IsCompleted { get; set; } = true;

        // Audit fields
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Bus Bus { get; set; } = null!;
        public User CreatedBy { get; set; } = null!;
        public User? UpdatedBy { get; set; }
    }
}
