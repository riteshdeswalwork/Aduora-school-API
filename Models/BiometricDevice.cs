using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class BiometricDevice
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string DeviceIdentifier { get; set; } = string.Empty;

        [StringLength(100)]
        public string? IpAddress { get; set; }

        public int Port { get; set; } = 80;

        [StringLength(100)]
        public string? MacAddress { get; set; }

        [StringLength(100)]
        public string? FirmwareVersion { get; set; }

        public int SyncIntervalSeconds { get; set; } = 60;

        [StringLength(200)]
        public string? ApiKey { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Active";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
