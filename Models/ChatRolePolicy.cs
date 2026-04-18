using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class ChatRolePolicy
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string AllowedRole { get; set; } = string.Empty;
        [Required]
        public bool IsActive { get; set; } = true;
    }
}
