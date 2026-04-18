using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class DocumentDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedBy { get; set; }
        public string? VerificationNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // User details
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string VerifiedByName { get; set; } = string.Empty;
    }

    public class UploadDocumentDTO
    {
        [Required]
        public IFormFile DocumentFile { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public Guid? UserId { get; set; } // For admin uploading for other users
    }

    public class UpdateDocumentDTO
    {
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class VerifyDocumentDTO
    {
        [Required]
        public Guid DocumentId { get; set; }
        
        [Required]
        public bool IsVerified { get; set; }
        
        [StringLength(500)]
        public string? VerificationNotes { get; set; }
    }

    public class DocumentListDTO
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string FileSizeFormatted => FormatFileSize(FileSize);
        
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
