using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class Enquiry
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        public string? ParentName { get; set; }

        [StringLength(20)]
        public string? ContactNumber { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? CurrentSchool { get; set; }

        [StringLength(50)]
        public string? SeekingClass { get; set; }

        [StringLength(150)]
        public string? CourseInterested { get; set; }

        [StringLength(100)]
        public string? Source { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; } // Hot, Warm, Cold

        [StringLength(50)]
        public string Status { get; set; } = "Open";

        public DateTime EnquiryDate { get; set; } = DateTime.UtcNow;
        public DateTime? FollowUpDate { get; set; }

        [StringLength(4000)]
        public string? FollowUpLog { get; set; } // JSON list of interactions

        [StringLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
