using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class BusExpense
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BusId { get; set; }
        public Bus? Bus { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string ExpenseType { get; set; } = string.Empty; // Maintenance, Tax, Insurance, Fine, etc.

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string? VendorName { get; set; }

        [StringLength(50)]
        public string? BillNumber { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

        public Guid? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedBy { get; set; }

        public Guid? AcademicYearId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
