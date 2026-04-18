using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class StudentIdCardRequest
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AcademicYearId { get; set; }
        public Guid? StudentId { get; set; }

        [StringLength(50)]
        public string? CardNumber { get; set; }

        [StringLength(50)]
        public string? CardTech { get; set; } // RFID, NFC, Standard

        public DateTime? CardExpiryDate { get; set; }

        [StringLength(100)]
        public string? ChipUid { get; set; }

        [StringLength(2000)]
        public string? MedicalData { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow.Date;

        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public Guid? RequestedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public DateTime? PrintedAt { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>Reason for request: New Enrollment, Loss of Card, Physical Damage, etc.</summary>
        [StringLength(100)]
        public string? IssueReason { get; set; }

        /// <summary>Photo URL for ID card design.</summary>
        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        [StringLength(10)]
        public string? BloodGroup { get; set; }

        [StringLength(500)]
        public string? Allergies { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
