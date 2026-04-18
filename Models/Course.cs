using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Course
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Credits { get; set; }

        // Foreign key to Department
        public Guid? DepartmentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign key
        public Guid? StaffId { get; set; }

        // Navigation properties
        public virtual Staff? Staff { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
    }
}
