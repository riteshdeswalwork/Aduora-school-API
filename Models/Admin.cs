using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Admin
    {
        [Key]
        public Guid Id { get; set; }

        public string AdminLevel { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        // Foreign key to User
        public Guid UserId { get; set; }

        // Navigation property to User
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        /// <summary>Not mapped. Used when creating/updating to assign role (UserRole).</summary>
        [NotMapped]
        public Guid? RoleId { get; set; }
    }
}
