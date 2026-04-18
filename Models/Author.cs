using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Author
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Biography { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }

        [StringLength(500)]
        public string? Website { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        // Social media links
        [StringLength(200)]
        public string? TwitterHandle { get; set; }

        [StringLength(200)]
        public string? FacebookProfile { get; set; }

        [StringLength(200)]
        public string? LinkedInProfile { get; set; }

        // Professional details
        [StringLength(500)]
        public string? Education { get; set; }

        [StringLength(1000)]
        public string? Awards { get; set; }

        [StringLength(1000)]
        public string? NotableWorks { get; set; }

        public bool IsActive { get; set; } = true;

        // Audit fields
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

        // Computed properties
        public bool IsAlive => !DateOfDeath.HasValue;
        public int? Age => DateOfBirth.HasValue ? 
            (DateOfDeath ?? DateTime.Now).Year - DateOfBirth.Value.Year : null;
        public int TotalBooks => BookAuthors?.Count ?? 0;
        public string DisplayName => Name;
        public string ShortBio => Biography?.Length > 100 ? 
            Biography.Substring(0, 100) + "..." : Biography ?? string.Empty;
    }

    // Junction table for many-to-many relationship between Book and Author
    public class BookAuthor
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid BookId { get; set; }

        [Required]
        public Guid AuthorId { get; set; }

        public bool IsPrimaryAuthor { get; set; } = false; // Main author for the book
        public int AuthorOrder { get; set; } = 1; // Order of authors for display

        [StringLength(100)]
        public string? Role { get; set; } // Author, Co-Author, Editor, Translator, etc.

        // Navigation properties
        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;

        [ForeignKey("AuthorId")]
        public virtual Author Author { get; set; } = null!;
    }
}
