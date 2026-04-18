using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class FeePayment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentFeeId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, Bank Transfer, Cheque

        [StringLength(100)]
        public string TransactionReference { get; set; } = string.Empty;

        [StringLength(100)]
        public string ReceiptNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;

        [Required]
        public Guid ReceivedById { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsVoided { get; set; }

        public DateTime? VoidedAtUtc { get; set; }

        public Guid? VoidedById { get; set; }

        [StringLength(500)]
        public string VoidReason { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("StudentFeeId")]
        public virtual StudentFee? StudentFee { get; set; }

        [ForeignKey("ReceivedById")]
        public virtual User? ReceivedBy { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYear { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<FeeReceipt> Receipts { get; set; } = new List<FeeReceipt>();
    }
}
