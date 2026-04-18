using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class LectureTopic
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? SubjectId { get; set; }

        public DateTime LectureDate { get; set; } = DateTime.UtcNow.Date;

        [Required]
        [StringLength(250)]
        public string Topic { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ChapterOrUnit { get; set; }

        [StringLength(4000)]
        public string? Details { get; set; }

        [StringLength(250)]
        public string? NotesUrl { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Planned";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
