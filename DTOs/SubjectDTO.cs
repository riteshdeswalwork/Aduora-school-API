using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.DTOs
{
    public class SubjectDTO
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Credits { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsCompulsory { get; set; } = false;

        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
