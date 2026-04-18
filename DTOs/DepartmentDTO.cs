using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.DTOs
{
    public class DepartmentDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department code is required")]
        [StringLength(20, ErrorMessage = "Department code cannot exceed 20 characters")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Head of Department name cannot exceed 100 characters")]
        public string? HeadOfDepartment { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateDepartmentDTO
    {
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department code is required")]
        [StringLength(20, ErrorMessage = "Department code cannot exceed 20 characters")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Head of Department name cannot exceed 100 characters")]
        public string? HeadOfDepartment { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string? Location { get; set; }
    }

    public class UpdateDepartmentDTO
    {
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department code is required")]
        [StringLength(20, ErrorMessage = "Department code cannot exceed 20 characters")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Head of Department name cannot exceed 100 characters")]
        public string? HeadOfDepartment { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
