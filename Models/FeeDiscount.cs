using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class FeeDiscount
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentFeeId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        [StringLength(100)]
        public string DiscountType { get; set; } = string.Empty; // Scholarship, Sibling, Merit, Financial Aid

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Percentage { get; set; } = 0;

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public Guid ApprovedById { get; set; }

        public DateTime ApprovedDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("StudentFeeId")]
        public virtual StudentFee? StudentFee { get; set; }

        [ForeignKey("ApprovedById")]
        public virtual User? ApprovedBy { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
    }
}
