using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class RolePermission
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        [Required]
        public Guid PermissionId { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        // Navigation Properties
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;

        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public virtual User? UpdatedBy { get; set; }

        // Computed Properties
        public string StatusDisplay => IsActive ? "Active" : "Inactive";
    }
}
