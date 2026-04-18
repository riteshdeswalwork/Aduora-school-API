using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class StaffLeaveApprover
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StaffId { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff Staff { get; set; } = null!;

        [Required]
        public Guid ApproverId { get; set; }

        [ForeignKey("ApproverId")]
        public virtual Staff Approver { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
