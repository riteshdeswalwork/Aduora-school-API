using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs;

/// <summary>Accepts both API model names and frontend Announcements.jsx field names.</summary>
public class AnnouncementUpsertDto
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public string? Priority { get; set; }

    public string? Category { get; set; }

    /// <summary>Frontend: targetAudience</summary>
    public string? TargetAudience { get; set; }

    public DateTime? PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    /// <summary>Frontend: startDate</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Frontend: endDate</summary>
    public DateTime? EndDate { get; set; }

    public bool? IsPublished { get; set; }

    /// <summary>Frontend: isActive</summary>
    public bool? IsActive { get; set; }

    public bool? IsFeatured { get; set; }

    public Guid? AcademicYearId { get; set; }

    public string? ImageUrl { get; set; }
}
