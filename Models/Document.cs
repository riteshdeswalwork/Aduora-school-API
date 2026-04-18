using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Document
    {
        public Guid Id { get; set; }

        [Required]
        public Guid AcademicYearId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty; // Aadhaar, PAN, Birth Certificate, etc.
        
        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty; // application/pdf, image/jpeg, etc.
        
        [Required]
        public byte[] DocumentData { get; set; } = Array.Empty<byte>();
        
        public long FileSize { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsVerified { get; set; } = false;
        
        public DateTime? VerifiedAt { get; set; }
        
        public Guid? VerifiedBy { get; set; } // Admin who verified
        
        [StringLength(500)]
        public string? VerificationNotes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        public virtual User User { get; set; } = null!;
        
        public virtual User? VerifiedByUser { get; set; }
    }
    
    public enum DocumentType
    {
        AadhaarCard,
        PanCard,
        BirthCertificate,
        MarkSheet,
        TransferCertificate,
        CasteCertificate,
        IncomeCertificate,
        PassportPhoto,
        Signature,
        Other
    }
}
