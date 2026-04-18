using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class Permission
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Code { get; set; } = string.Empty; // Unique identifier like "users.create", "students.view"

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Module { get; set; } = string.Empty; // e.g., "Users", "Students", "Classes"

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // e.g., "Create", "Read", "Update", "Delete"

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsSystemPermission { get; set; } = false; // System permissions cannot be deleted

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        // Navigation Properties
        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public virtual User? UpdatedBy { get; set; }

        /// <summary>Optional parent for sub-permissions (e.g. page.students.add under page.students).</summary>
        public Guid? ParentPermissionId { get; set; }

        [ForeignKey("ParentPermissionId")]
        [JsonIgnore]
        public virtual Permission? Parent { get; set; }

        [JsonIgnore]
        public virtual ICollection<Permission> Children { get; set; } = new List<Permission>();

        [JsonIgnore]
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        // Computed Properties
        public int RoleCount => RolePermissions?.Count(rp => rp.IsActive) ?? 0;
        public string StatusDisplay => IsActive ? "Active" : "Inactive";
        public string TypeDisplay => IsSystemPermission ? "System Permission" : "Custom Permission";
        public string FullName => $"{Module}.{Action}";
    }
}
