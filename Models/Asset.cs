using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class Asset
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [StringLength(100)]
        public string? TagId { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }

        public decimal? PurchaseCost { get; set; }
        public decimal? DepreciatedValue { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(200)]
        public string? SupplierContact { get; set; }

        [StringLength(50)]
        public string? LifespanEstimate { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Active";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
