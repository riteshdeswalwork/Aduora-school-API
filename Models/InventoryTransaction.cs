using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class InventoryTransaction
    {
        [Key]
        public Guid Id { get; set; }

        public Guid InventoryItemId { get; set; }

        [StringLength(10)]
        public string TransactionType { get; set; } = "IN";

        public decimal Quantity { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Remarks { get; set; }

        public Guid? PerformedByUserId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
