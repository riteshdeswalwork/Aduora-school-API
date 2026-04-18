using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class GalleryMedia
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(100)]
        public string? AlbumCategory { get; set; }

        [Required]
        [StringLength(20)]
        public string MediaType { get; set; } = "Photo";

        [StringLength(500)]
        public string? Url { get; set; }

        [StringLength(100)]
        public string? VisibilityTier { get; set; }

        public DateTime? EventDate { get; set; }

        public Guid? UploadedByUserId { get; set; }

        [StringLength(200)]
        public string? PhotographerMetadata { get; set; }

        public int MediaDensity { get; set; }
        public long TotalAssetSize { get; set; }

        public bool IsPublic { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
