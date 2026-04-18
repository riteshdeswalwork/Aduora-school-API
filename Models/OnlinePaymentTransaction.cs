using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class OnlinePaymentTransaction
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }
        public Guid? StudentId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public decimal ConvenienceFees { get; set; }
        public decimal TaxBreakdown { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "INR";

        [StringLength(100)]
        public string? PaymentFor { get; set; }

        [StringLength(100)]
        public string? Gateway { get; set; }

        [StringLength(150)]
        public string? TransactionReference { get; set; }

        [StringLength(150)]
        public string? MerchantRefId { get; set; }

        [StringLength(150)]
        public string? RefundReference { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Initiated";

        [StringLength(30)]
        public string? SettlementStatus { get; set; }

        public int LatencyMs { get; set; }

        public DateTime? PaidAt { get; set; }

        [StringLength(100)]
        public string? ReceiptNumber { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? LedgerCategory { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
