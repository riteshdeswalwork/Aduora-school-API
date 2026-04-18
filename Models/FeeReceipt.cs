using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class FeeReceipt
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PaymentId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        [StringLength(50)]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Required]
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid IssuedById { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int ReprintCount { get; set; }

        public DateTime? LastReprintAtUtc { get; set; }

        // Navigation properties
        [ForeignKey("PaymentId")]
        public virtual FeePayment? Payment { get; set; }

        [ForeignKey("IssuedById")]
        public virtual User? IssuedBy { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
    }
}
