using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class BusFuelRecord
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BusId { get; set; }
        public Bus? Bus { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RatePerUnit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string? ReceiptNumber { get; set; }

        [StringLength(100)]
        public string? FuelStation { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public Guid? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedBy { get; set; }

        public Guid? AcademicYearId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
