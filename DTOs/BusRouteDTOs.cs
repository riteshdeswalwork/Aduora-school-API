using System.ComponentModel.DataAnnotations;

namespace sms.api.DTOs
{
    public class BusRouteDTO
    {
        public Guid Id { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public Guid BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string BusName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal TotalDistance { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
        public TimeSpan? MorningStartTime { get; set; }
        public TimeSpan? MorningEndTime { get; set; }
        public TimeSpan? EveningStartTime { get; set; }
        public TimeSpan? EveningEndTime { get; set; }
        public bool IsActive { get; set; }
        public string? RouteWaypoints { get; set; } // JSON string with route coordinates
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Related data
        public List<BusStopDTO> BusStops { get; set; } = new List<BusStopDTO>();
        public int TotalStops { get; set; }
        public int TotalStudents { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CreateBusRouteDTO
    {
        [Required]
        [StringLength(100)]
        public string RouteName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string RouteCode { get; set; } = string.Empty;

        [Required]
        public Guid BusId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 1000)]
        public decimal TotalDistance { get; set; }

        public TimeSpan EstimatedDuration { get; set; }

        [Required]
        public TimeSpan MorningStartTime { get; set; }

        [Required]
        public TimeSpan MorningEndTime { get; set; }

        [Required]
        public TimeSpan EveningStartTime { get; set; }

        [Required]
        public TimeSpan EveningEndTime { get; set; }

        [StringLength(10000)]
        public string? RouteWaypoints { get; set; } // JSON string with route coordinates
    }

    public class UpdateBusRouteDTO
    {
        [StringLength(100)]
        public string? RouteName { get; set; }

        public Guid? BusId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 1000)]
        public decimal? TotalDistance { get; set; }

        public TimeSpan? EstimatedDuration { get; set; }
        public TimeSpan? MorningStartTime { get; set; }
        public TimeSpan? MorningEndTime { get; set; }
        public TimeSpan? EveningStartTime { get; set; }
        public TimeSpan? EveningEndTime { get; set; }
        public bool? IsActive { get; set; }

        [StringLength(10000)]
        public string? RouteWaypoints { get; set; } // JSON string with route coordinates
    }

    public class BusStopDTO
    {
        public Guid Id { get; set; }
        public string StopName { get; set; } = string.Empty;
        public string StopCode { get; set; } = string.Empty;
        public Guid? RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public int StopOrder { get; set; }
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Landmark { get; set; }
        public decimal DistanceFromSchool { get; set; }
        public TimeSpan? MorningPickupTime { get; set; }
        public TimeSpan? EveningDropTime { get; set; }
        public decimal MonthlyFee { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Related data
        public int TotalStudents { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }

    public class CreateBusStopDTO
    {
        [Required]
        [StringLength(100)]
        public string StopName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string StopCode { get; set; } = string.Empty;

        /// <summary>Optional. When null, stop can be added first and assigned to a route later.</summary>
        public Guid? RouteId { get; set; }

        [Range(1, 100)]
        public int StopOrder { get; set; } = 1;

        [StringLength(50)]
        public string Latitude { get; set; } = string.Empty;

        [StringLength(50)]
        public string Longitude { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Landmark { get; set; }

        [Range(0, 100)]
        public decimal DistanceFromSchool { get; set; }

        public TimeSpan? MorningPickupTime { get; set; }

        public TimeSpan? EveningDropTime { get; set; }

        [Range(0, 10000)]
        public decimal MonthlyFee { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateBusStopDTO
    {
        [StringLength(100)]
        public string? StopName { get; set; }

        public Guid? RouteId { get; set; }

        [Range(1, 100)]
        public int? StopOrder { get; set; }

        [StringLength(50)]
        public string? Latitude { get; set; }

        [StringLength(50)]
        public string? Longitude { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Landmark { get; set; }

        [Range(0, 100)]
        public decimal? DistanceFromSchool { get; set; }

        public TimeSpan? MorningPickupTime { get; set; }
        public TimeSpan? EveningDropTime { get; set; }

        [Range(0, 10000)]
        public decimal? MonthlyFee { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }


}
