using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class FeeStructure
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string FeeType { get; set; } = string.Empty; // Monthly, Quarterly, Yearly, One-time

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        [Required]
        public Guid AcademicYearId { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>For Yearly/One-time: split into N instalment demands (2+). Null or 1 = single demand.</summary>
        public int? InstallmentCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign key to Class. Null = common fee, applies to every class.
        public Guid? ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }

        /// <summary>Populated by EF when included; omit on API create/update JSON (use <see cref="AcademicYearId"/> only).</summary>
        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYearNavigation { get; set; }

        public virtual ICollection<FeeStructureLine> Lines { get; set; } = new List<FeeStructureLine>();

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<StudentFee> StudentFees { get; set; } = new List<StudentFee>();
    }
}
