using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class Scholarship
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Provider { get; set; }

        public decimal? Amount { get; set; }

        [StringLength(100)]
        public string? GrantType { get; set; }

        [StringLength(100)]
        public string? DisbursementMode { get; set; }

        [StringLength(50)]
        public string? RenewalFrequency { get; set; }

        public decimal MinimumGpaRequirement { get; set; }

        [StringLength(2000)]
        public string? EligibilityCriteria { get; set; }

        [StringLength(2000)]
        public string? DocHashes { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
