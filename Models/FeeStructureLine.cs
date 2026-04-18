using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    /// <summary>OpenEduCat-style component line under a class fee head (e.g. Tuition, Transport, Lab).</summary>
    public class FeeStructureLine
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid FeeStructureId { get; set; }

        [Required]
        [StringLength(50)]
        public string LineCode { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int SortOrder { get; set; }

        /// <summary>Applied when auto-generating demands: initial concession % on this line (0–100).</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal DefaultConcessionPercent { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("FeeStructureId")]
        [JsonIgnore]
        public virtual FeeStructure? FeeStructure { get; set; }

        [JsonIgnore]
        public virtual ICollection<StudentFee> StudentFees { get; set; } = new List<StudentFee>();
    }
}
