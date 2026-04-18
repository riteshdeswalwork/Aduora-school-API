using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class SchoolInfo
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string SchoolName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string Website { get; set; } = string.Empty;

        [StringLength(100)]
        public string Principal { get; set; } = string.Empty;

        [StringLength(100)]
        public string VicePrincipal { get; set; } = string.Empty;

        public int EstablishedYear { get; set; }

        [StringLength(50)]
        public string AffiliationBoard { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Mission { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Vision { get; set; } = string.Empty;

        [StringLength(200)]
        public string LogoUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
