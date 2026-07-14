using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using sms.api.Constants;

namespace sms.api.Models
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Data visibility: Global, AssignedClasses (class teacher), or OwnOnly.</summary>
        public DataScopeType DataScope { get; set; } = DataScopeType.AssignedClasses;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsSystemRole { get; set; } = false; // System roles cannot be deleted

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        // Navigation Properties
        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public virtual User? UpdatedBy { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        [JsonIgnore]
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        // Computed Properties
        public int UserCount => UserRoles?.Count(ur => ur.IsActive) ?? 0;
        public int PermissionCount => RolePermissions?.Count(rp => rp.IsActive) ?? 0;
        public string StatusDisplay => IsActive ? "Active" : "Inactive";
        public string TypeDisplay => IsSystemRole ? "System Role" : "Custom Role";
    }
}
