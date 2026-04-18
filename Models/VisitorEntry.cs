using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class VisitorEntry
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(150)]
        public string VisitorName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ContactNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(250)]
        public string? Purpose { get; set; }

        public Guid? HostStaffId { get; set; }
        public Guid? DepartmentId { get; set; }

        [StringLength(50)]
        public string? EntryGate { get; set; }

        public int NumberOfPersons { get; set; } = 1;

        [StringLength(50)]
        public string? IdType { get; set; } // Aadhar, PAN, etc.

        [StringLength(50)]
        public string? IdNumber { get; set; }

        [StringLength(150)]
        public string? PersonToMeet { get; set; }

        [StringLength(50)]
        public string? VehicleNumber { get; set; }

        [StringLength(50)]
        public string? BadgeNumber { get; set; }

        public DateTime TimeIn { get; set; } = DateTime.UtcNow;
        public DateTime? TimeOut { get; set; }

        /// <summary>In = on premises, Out = checked out, Cancelled = refused/cancelled</summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "In";

        public Guid? CheckInById { get; set; }
        public Guid? CheckOutById { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
