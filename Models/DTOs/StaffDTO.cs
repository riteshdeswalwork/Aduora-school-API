using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class StaffDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public Guid? DesignationId { get; set; }
        public DateTime DateOfJoining { get; set; }
        public string Qualification { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string Address { get; set; } = string.Empty;
        public UserDTO? User { get; set; }
    }
    public class CreateStaffDTO
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public Guid? DesignationId { get; set; }

        public string EmployeeId { get; set; } = string.Empty;

        public DateTime DateOfJoining { get; set; } = DateTime.UtcNow;

        [Required]
        public string Qualification { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number")]
        public decimal Salary { get; set; }

        // Profile photo as base64 string
        public string? ProfilePicture { get; set; }

        /// <summary>Optional. When provided, assigns this role to the user and creates UserRole.</summary>
        public Guid? RoleId { get; set; }
    }

    public class UpdateStaffDTO
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public Guid? DesignationId { get; set; }

        public string EmployeeId { get; set; } = string.Empty;

        public DateTime DateOfJoining { get; set; }

        [Required]
        public string Qualification { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number")]
        public decimal Salary { get; set; }

        // Profile photo as base64 string
        public string? ProfilePicture { get; set; }

        /// <summary>Optional. When provided, updates user's role (UserRole and User.Role).</summary>
        public Guid? RoleId { get; set; }

        /// <summary>Optional. When provided (min 6 chars), updates the linked user's password.</summary>
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }
    }
}
