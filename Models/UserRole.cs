using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class UserRole
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }

        public Guid? AssignedByUserId { get; set; }
        public Guid? RevokedByUserId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("AssignedByUserId")]
        public virtual User? AssignedBy { get; set; }

        [ForeignKey("RevokedByUserId")]
        public virtual User? RevokedBy { get; set; }

        // Computed Properties
        public string StatusDisplay => IsActive ? "Active" : "Revoked";
        public DateTime EffectiveDate => IsActive ? AssignedAt : (RevokedAt ?? AssignedAt);
    }
}
