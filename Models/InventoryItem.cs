using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class InventoryItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(50)]
        public string? Sku { get; set; }

        public decimal Quantity { get; set; } = 0;

        public decimal CriticalThreshold { get; set; } = 5;

        [StringLength(20)]
        public string Unit { get; set; } = "pcs";

        public decimal? ReorderLevel { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(200)]
        public string? BinLocation { get; set; }

        [StringLength(200)]
        public string? CurrentSupplier { get; set; }

        public decimal UnitCost { get; set; }

        [StringLength(50)]
        public string? DepletionRate { get; set; }

        [StringLength(100)]
        public string? BatchNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
