using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class Hostel
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? BuildingName { get; set; }

        public Guid? WardenId { get; set; }

        [StringLength(50)]
        public string? SecurityLevel { get; set; }

        [StringLength(500)]
        public string? AmenityTags { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        /// <summary>Boys, Girls, or Co-ed</summary>
        [StringLength(30)]
        public string? HostelType { get; set; }

        /// <summary>Warden or office contact number</summary>
        [StringLength(30)]
        public string? ContactNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
