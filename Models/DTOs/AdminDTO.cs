using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class CreateAdminDTO
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

        public string AdminLevel { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        /// <summary>Optional. When provided, assigns this role to the user and creates UserRole.</summary>
        public Guid? RoleId { get; set; }

        /// <summary>Optional. Base64 profile photo data (e.g. data:image/jpeg;base64,...).</summary>
        public string? ProfilePhotoData { get; set; }
    }

    public class UpdateAdminDTO
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

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string AdminLevel { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        /// <summary>Optional. When provided, updates user's role (UserRole and User.Role).</summary>
        public Guid? RoleId { get; set; }

        /// <summary>Optional. Base64 profile photo data; set to empty string to remove photo.</summary>
        public string? ProfilePhotoData { get; set; }
    }
}
