using System.ComponentModel.DataAnnotations;

namespace sms.api.DTOs
{
    public class BusDTO
    {
        public Guid Id { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string BusName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }
        public string? ConductorName { get; set; }
        public string? ConductorPhone { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? FuelType { get; set; }
        public decimal? FuelCapacity { get; set; }
        public decimal? Mileage { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsAvailable { get; set; }
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }
        public DateTime? FitnessExpiryDate { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Related data
        public List<BusRouteDTO> Routes { get; set; } = new List<BusRouteDTO>();
        public int TotalStudents { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }

    public class CreateBusDTO
    {
        [Required]
        [StringLength(20)]
        public string BusNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string BusName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        [Range(10, 100)]
        public int Capacity { get; set; }

        [StringLength(100)]
        public string? DriverName { get; set; }

        [StringLength(15)]
        public string? DriverPhone { get; set; }

        [StringLength(100)]
        public string? ConductorName { get; set; }

        [StringLength(15)]
        public string? ConductorPhone { get; set; }

        [StringLength(50)]
        public string? Make { get; set; }

        [StringLength(50)]
        public string? Model { get; set; }

        [Range(1990, 2030)]
        public int? Year { get; set; }

        [StringLength(20)]
        public string? FuelType { get; set; }

        [Range(0, 1000)]
        public decimal? FuelCapacity { get; set; }

        [Range(0, 50)]
        public decimal? Mileage { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime? InsuranceExpiryDate { get; set; }
        public DateTime? FitnessExpiryDate { get; set; }
    }

    public class UpdateBusDTO
    {
        [StringLength(100)]
        public string? BusName { get; set; }

        [Range(10, 100)]
        public int? Capacity { get; set; }

        [StringLength(100)]
        public string? DriverName { get; set; }

        [StringLength(15)]
        public string? DriverPhone { get; set; }

        [StringLength(100)]
        public string? ConductorName { get; set; }

        [StringLength(15)]
        public string? ConductorPhone { get; set; }

        [StringLength(50)]
        public string? Make { get; set; }

        [StringLength(50)]
        public string? Model { get; set; }

        [Range(1990, 2030)]
        public int? Year { get; set; }

        [StringLength(20)]
        public string? FuelType { get; set; }

        [Range(0, 1000)]
        public decimal? FuelCapacity { get; set; }

        [Range(0, 50)]
        public decimal? Mileage { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
        public bool? IsAvailable { get; set; }

        public DateTime? InsuranceExpiryDate { get; set; }
        public DateTime? FitnessExpiryDate { get; set; }
    }

    public class BusLocationUpdateDTO
    {
        [Required]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }
    }

    public class BusMaintenanceDTO
    {
        public Guid Id { get; set; }
        public Guid BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime MaintenanceDate { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public string? ServiceProvider { get; set; }
        public string? ServiceProviderPhone { get; set; }
        public int? Odometer { get; set; }
        public DateTime? NextServiceDate { get; set; }
        public string? PartsReplaced { get; set; }
        public string? Remarks { get; set; }
        public bool IsCompleted { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBusMaintenanceDTO
    {
        [Required]
        public Guid BusId { get; set; }

        [Required]
        [StringLength(100)]
        public string MaintenanceType { get; set; } = string.Empty;

        [Required]
        public DateTime MaintenanceDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal Cost { get; set; }

        [StringLength(100)]
        public string? ServiceProvider { get; set; }

        [StringLength(15)]
        public string? ServiceProviderPhone { get; set; }

        public int? Odometer { get; set; }
        public DateTime? NextServiceDate { get; set; }

        [StringLength(500)]
        public string? PartsReplaced { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class BusExpenseDTO
    {
        public Guid Id { get; set; }
        public Guid BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string ExpenseType { get; set; } = string.Empty;
        public DateTime ExpenseDate { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? VendorName { get; set; }
        public string? BillNumber { get; set; }
        public string? Remarks { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBusExpenseDTO
    {
        [Required]
        public Guid BusId { get; set; }

        [Required]
        [StringLength(100)]
        public string ExpenseType { get; set; } = string.Empty;

        [Required]
        public DateTime ExpenseDate { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? VendorName { get; set; }

        [StringLength(50)]
        public string? BillNumber { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
