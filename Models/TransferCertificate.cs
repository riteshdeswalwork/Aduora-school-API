using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class TransferCertificate
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }
        public Guid? StudentId { get; set; }

        [ForeignKey("StudentId")]
        [JsonIgnore]
        public virtual Student? Student { get; set; }

        [StringLength(50)]
        public string? TcNumber { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow.Date;

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(100)]
        public string? ClearanceStatus { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public Guid? GeneratedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        [StringLength(2000)]
        public string? ApprovalChain { get; set; }

        [StringLength(500)]
        public string? HashVerification { get; set; }

        [StringLength(2000)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Real-life TC fields
        public DateTime? AdmissionDate { get; set; }
        public DateTime? LeavingDate { get; set; }

        [StringLength(50)]
        public string? LastClassStudied { get; set; }

        [StringLength(50)]
        public string? PromotedToClass { get; set; }

        [StringLength(500)]
        public string? SubjectsStudied { get; set; }

        [StringLength(50)]
        public string? CharacterConduct { get; set; }

        [StringLength(20)]
        public string? AcademicPerformance { get; set; }

        public bool AnyDuesPending { get; set; }
        public bool LibraryClearance { get; set; } = true;
        public bool LabClearance { get; set; } = true;
        public bool TransportClearance { get; set; } = true;
    }
}
