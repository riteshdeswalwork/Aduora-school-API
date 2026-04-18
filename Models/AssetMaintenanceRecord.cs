using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class AssetMaintenanceRecord
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AssetId { get; set; }

        public DateTime ServiceDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime? NextDueDate { get; set; }

        [StringLength(150)]
        public string? Vendor { get; set; }

        [StringLength(150)]
        public string? VendorContact { get; set; }

        public decimal? ServiceCost { get; set; }

        [Range(0, 5)]
        public int PostServiceHealthGrade { get; set; }

        [StringLength(2000)]
        public string? MaintenanceChecklist { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Completed";

        public bool IsActive { get; set; } = true;
    }
}
