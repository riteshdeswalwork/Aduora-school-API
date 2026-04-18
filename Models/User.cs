using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // Admin, Staff, Student

        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        // Profile Photo as BLOB
        public byte[]? ProfilePhotoData { get; set; }

        [StringLength(100)]
        public string? ProfilePhotoContentType { get; set; }

        [StringLength(255)]
        public string? ProfilePhotoFileName { get; set; }

        // Backward compatibility
        public string? ProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual Staff? Staff { get; set; }

        [JsonIgnore]
        public virtual Student? Student { get; set; }

        [JsonIgnore]
        public virtual Admin? Admin { get; set; }
    }
}
