using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class BookCategory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Fiction, Non-Fiction, Reference, etc.

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string Code { get; set; } = string.Empty; // FIC, NF, REF, etc.

        [StringLength(7)]
        public string ColorCode { get; set; } = "#007bff"; // For UI display

        public bool IsActive { get; set; } = true;

        // Library settings
        public int MaxIssueDays { get; set; } = 14; // Default issue period
        public int MaxRenewalCount { get; set; } = 2; // Maximum renewals allowed
        public bool IsReferenceOnly { get; set; } = false; // Cannot be issued (reference books)

        [Column(TypeName = "decimal(10,2)")]
        public decimal FinePerDay { get; set; } = 1.00m; // Fine for overdue books

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
        public int AvailableBooks => Books?.Count(b => b.IsAvailable) ?? 0;
        public int IssuedBooks => Books?.Count(b => !b.IsAvailable) ?? 0;
    }
}
