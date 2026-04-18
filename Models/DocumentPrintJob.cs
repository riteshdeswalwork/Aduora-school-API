using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models
{
    public class DocumentPrintJob
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [StringLength(200)]
        public string? TemplateName { get; set; }

        [StringLength(100)]
        public string? TargetReference { get; set; }

        [StringLength(30)]
        public string JobPriority { get; set; } = "Normal";

        [StringLength(30)]
        public string? PaperType { get; set; }

        [StringLength(30)]
        public string? ColorMode { get; set; }

        public bool IsDuplex { get; set; }

        [StringLength(50)]
        public string? PrintQuality { get; set; }

        [StringLength(50)]
        public string? PrinterNodeIp { get; set; }

        public int BatchSize { get; set; } = 1;

        [StringLength(30)]
        public string Status { get; set; } = "Queued";

        public Guid? RequestedByUserId { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PrintedAt { get; set; }
        public DateTime? EstimatedCompletion { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
