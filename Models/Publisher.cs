using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Publisher
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        // Contact Information
        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Website { get; set; }

        // Business Information
        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        public string? ContactPhone { get; set; }

        [StringLength(200)]
        public string? ContactEmail { get; set; }

        // Financial Information
        [StringLength(50)]
        public string? TaxId { get; set; }

        [StringLength(100)]
        public string? BankAccount { get; set; }

        [StringLength(200)]
        public string? PaymentTerms { get; set; }

        // Publisher Details
        public DateTime? EstablishedDate { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(1000)]
        public string? Specialization { get; set; } // Academic, Fiction, Technical, etc.

        public bool IsActive { get; set; } = true;

        // Rating and Reviews
        [Range(1, 5)]
        public decimal? Rating { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Audit fields
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        // Computed properties
        public int TotalBooks => Books?.Count ?? 0;
        public int ActiveBooks => Books?.Count(b => b.IsActive) ?? 0;
        public string DisplayName => Name;
        public string FullAddress => $"{Address}, {City}, {State}, {Country} - {PostalCode}".Trim(' ', ',', '-');
        public bool HasContactInfo => !string.IsNullOrEmpty(Phone) || !string.IsNullOrEmpty(Email);
        public string PrimaryContact => !string.IsNullOrEmpty(ContactPerson) ? 
            $"{ContactPerson} ({ContactPhone ?? ContactEmail})" : 
            Phone ?? Email ?? "No contact info";
    }
}
