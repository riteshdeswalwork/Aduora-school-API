using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class CreateStudentLeaveDTO
    {
        [Required]
        public Guid StudentId { get; set; }

        public Guid? ClassId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class CreateStaffLeaveDTO
    {
        [Required]
        public Guid StaffId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public string LeaveType { get; set; } = "Regular";
    }

    public class UpdateLeaveStatusDTO
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Approved or Rejected

        public string? RejectionReason { get; set; }
    }

    public class CreateStaffLeaveApproverDTO
    {
        [Required]
        public Guid StaffId { get; set; }

        [Required]
        public Guid ApproverId { get; set; }
    }

    public class StudentLeaveResponseDTO
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid? ClassId { get; set; }
        public string? ClassName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class StaffLeaveResponseDTO
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
