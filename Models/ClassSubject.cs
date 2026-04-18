using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class ClassSubject
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        [Required]
        public Guid SubjectId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        public bool IsCompulsory { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
    }
}
