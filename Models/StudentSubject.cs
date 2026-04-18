using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class StudentSubject
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid SubjectId { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if this subject is optional for this specific student
        /// (beyond the minimum required subjects)
        /// </summary>
        public bool IsOptional { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; } = null!;

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYearNavigation { get; set; } = null!;
    }
}
