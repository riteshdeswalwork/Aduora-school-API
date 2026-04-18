using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class CreateSubjectDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Credits { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? StaffId { get; set; }
    }

    public class UpdateSubjectDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Guid? StaffId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
