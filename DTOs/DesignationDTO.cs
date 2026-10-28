using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using sms.api.Models.DTOs;

namespace sms.api.DTOs
{
    public class DesignationDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Designation name is required")]
        [StringLength(50, ErrorMessage = "Designation name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public Guid DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Staff statistics
        public int TotalStaff { get; set; }
        public int ActiveStaff { get; set; }
        public int InactiveStaff { get; set; }

        // Staff collection (only populated for GetById, not GetAll)
        public List<StaffDTO>? Staff { get; set; }
    }

    public class CreateDesignationDTO
    {
        [Required(ErrorMessage = "Designation name is required")]
        [StringLength(50, ErrorMessage = "Designation name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? Description { get; set; }

        public Guid? DepartmentId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateDesignationDTO
    {
        [Required(ErrorMessage = "Designation name is required")]
        [StringLength(50, ErrorMessage = "Designation name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? Description { get; set; }

        public Guid? DepartmentId { get; set; }

        public bool IsActive { get; set; }
    }
}
