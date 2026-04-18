using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class HomeworkAssignment
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }

        public Guid? ClassId { get; set; }
        public Guid? SubjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(3000)]
        public string? Description { get; set; }

        public DateTime? IssuedDate { get; set; }
        public DateTime? DueDate { get; set; }

        [StringLength(250)]
        public string? AttachmentUrl { get; set; }

        public Guid? AssignedById { get; set; }
        public int? MaxMarks { get; set; }

        [StringLength(1000)]
        public string? InstructionsForParents { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Published";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
