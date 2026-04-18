using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class Department
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? HeadOfDepartment { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();
        public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
