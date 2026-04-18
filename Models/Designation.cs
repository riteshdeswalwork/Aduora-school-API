using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Designation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        public Guid? DepartmentId { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;

        public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
    }
}
