using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class UploadProfilePhotoDTO
    {
        [Required]
        public IFormFile PhotoFile { get; set; } = null!;
    }

    public class ProfilePhotoResponseDTO
    {
        public Guid UserId { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
    }

    public class ProfilePhotoDataDTO
    {
        public byte[] PhotoData { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
