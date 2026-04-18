using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class HostelRoom
    {
        [Key]
        public Guid Id { get; set; }

        public Guid HostelId { get; set; }

        [Required]
        [StringLength(50)]
        public string RoomNumber { get; set; } = string.Empty;

        public int Floor { get; set; }

        public int Capacity { get; set; } = 1;

        public bool IsAc { get; set; }

        public decimal MonthlyRent { get; set; }

        [StringLength(2000)]
        public string? FurnitureJson { get; set; }

        [StringLength(50)]
        public string? RoomType { get; set; }

        /// <summary>Available, Occupied, Maintenance, Reserved</summary>
        [StringLength(30)]
        public string? Status { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
