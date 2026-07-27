using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class UserNotificationPreference
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public bool EnablePush { get; set; } = true;
        public bool EnableEmail { get; set; } = true;
        public bool EnableSms { get; set; } = true;
        public bool EnableInApp { get; set; } = true;

        public bool MuteApprovals { get; set; }
        public bool MuteFees { get; set; }
        public bool MuteAttendance { get; set; }
        public bool MuteSystem { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
