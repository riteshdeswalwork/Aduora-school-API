using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;
using System.Security.Claims;

namespace sms.api.Controllers.Shared
{
    [Route("api/announcements")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementManager _announcementManager;
        private readonly ApplicationDbContext _db;

        public AnnouncementController(IAnnouncementManager announcementManager, ApplicationDbContext db)
        {
            _announcementManager = announcementManager;
            _db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublishedAnnouncements()
        {
            try
            {
                var announcements = await _announcementManager.GetPublishedAsync();
                return Ok(announcements.Select(MapToClient));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeaturedAnnouncements()
        {
            try
            {
                var announcements = await _announcementManager.GetFeaturedAsync();
                return Ok(announcements.Select(MapToClient));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAnnouncements()
        {
            try
            {
                var announcements = await _announcementManager.GetAllAsync();
                return Ok(announcements.Select(MapToClient));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnnouncementById(Guid id)
        {
            try
            {
                var announcement = await _announcementManager.GetByIdAsync(id);
                if (announcement == null)
                    return NotFound(new { message = "Announcement not found" });

                return Ok(MapToClient(announcement));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnnouncementsByCategory(string category)
        {
            try
            {
                var announcements = await _announcementManager.GetByCategoryAsync(category);
                return Ok(announcements.Select(MapToClient));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("priority/{priority}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnnouncementsByPriority(string priority)
        {
            try
            {
                var announcements = await _announcementManager.GetByPriorityAsync(priority);
                return Ok(announcements.Select(MapToClient));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAnnouncement([FromBody] AnnouncementUpsertDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var announcement = await MapDtoToEntityAsync(dto, Guid.Parse(userId), existing: null);
                var result = await _announcementManager.CreateAsync(announcement);
                return CreatedAtAction(nameof(GetAnnouncementById), new { id = result.Id }, MapToClient(result));
            }
            catch (Exception ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = detail });
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAnnouncement(Guid id, [FromBody] AnnouncementUpsertDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existing = await _announcementManager.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = "Announcement not found" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var creator = existing.CreatedBy;
                if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var uid))
                    creator = uid;

                var announcement = await MapDtoToEntityAsync(dto, creator, existing);
                announcement.Id = id;
                var result = await _announcementManager.UpdateAsync(announcement);
                return Ok(MapToClient(result));
            }
            catch (Exception ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = detail });
            }
        }

        [HttpPut("{id:guid}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateAnnouncement(Guid id)
        {
            try
            {
                var result = await _announcementManager.SetPublishedAsync(id, true);
                if (result == null)
                    return NotFound(new { message = "Announcement not found" });
                return Ok(MapToClient(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{id:guid}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateAnnouncement(Guid id)
        {
            try
            {
                var result = await _announcementManager.SetPublishedAsync(id, false);
                if (result == null)
                    return NotFound(new { message = "Announcement not found" });
                return Ok(MapToClient(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAnnouncement(Guid id)
        {
            try
            {
                var result = await _announcementManager.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = "Announcement not found" });

                return Ok(new { message = "Announcement deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        private async Task<Announcement> MapDtoToEntityAsync(AnnouncementUpsertDto dto, Guid createdBy, Announcement? existing)
        {
            static DateTime? ParseDate(DateTime? value)
            {
                if (!value.HasValue || value.Value == default || value.Value.Year < 2000)
                    return null;
                return DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
            }

            var publishDate = ParseDate(dto.PublishDate)
                              ?? ParseDate(dto.StartDate)
                              ?? existing?.PublishDate
                              ?? DateTime.UtcNow;

            var expiry = ParseDate(dto.ExpiryDate) ?? ParseDate(dto.EndDate) ?? existing?.ExpiryDate;
            var isPublished = dto.IsPublished ?? dto.IsActive ?? existing?.IsPublished ?? true;
            var category = !string.IsNullOrWhiteSpace(dto.Category)
                ? dto.Category!
                : (!string.IsNullOrWhiteSpace(dto.TargetAudience) ? dto.TargetAudience! : (existing?.Category ?? "General"));

            // Prefer year from request header (selected in UI), then dto, then current year
            Guid? yearId = _db.CurrentAcademicYearId
                           ?? dto.AcademicYearId
                           ?? existing?.AcademicYearId;

            if (yearId == null || yearId == Guid.Empty)
            {
                yearId = await _db.AcademicYears.AsNoTracking()
                    .Where(y => y.IsCurrentYear || y.IsActive)
                    .OrderByDescending(y => y.IsCurrentYear)
                    .Select(y => (Guid?)y.Id)
                    .FirstOrDefaultAsync()
                    ?? await _db.AcademicYears.AsNoTracking().Select(y => (Guid?)y.Id).FirstOrDefaultAsync();
            }

            if (yearId == null || yearId == Guid.Empty)
                throw new InvalidOperationException("No academic year found. Create/select an academic year first.");

            var yearExists = await _db.AcademicYears.AsNoTracking().AnyAsync(y => y.Id == yearId.Value);
            if (!yearExists)
                throw new InvalidOperationException("Selected academic year is invalid.");

            // Ensure CreatedBy exists in Users (FK)
            var creatorOk = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == createdBy);
            if (!creatorOk)
            {
                var fallback = await _db.Users.AsNoTracking()
                    .Where(u => u.Role == "Admin" && u.IsActive)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync();
                if (fallback == null)
                    throw new InvalidOperationException("Creator user not found.");
                createdBy = fallback.Value;
            }

            return new Announcement
            {
                Id = existing?.Id ?? Guid.Empty,
                Title = (dto.Title ?? "").Trim(),
                Content = (dto.Content ?? "").Trim(),
                Priority = string.IsNullOrWhiteSpace(dto.Priority) ? (existing?.Priority ?? "Normal") : dto.Priority.Trim(),
                Category = category.Trim(),
                PublishDate = publishDate,
                ExpiryDate = expiry,
                IsPublished = isPublished,
                IsFeatured = dto.IsFeatured ?? existing?.IsFeatured ?? false,
                ImageUrl = dto.ImageUrl ?? existing?.ImageUrl ?? string.Empty,
                AcademicYearId = yearId.Value,
                CreatedBy = existing?.CreatedBy ?? createdBy,
                CreatedAt = existing?.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = existing == null ? null : DateTime.UtcNow,
                Creator = null,
                AcademicYear = null!
            };
        }

        private static object MapToClient(Announcement a) => new
        {
            a.Id,
            a.Title,
            a.Content,
            a.Priority,
            a.Category,
            targetAudience = a.Category,
            publishDate = a.PublishDate,
            expiryDate = a.ExpiryDate,
            startDate = a.PublishDate,
            endDate = a.ExpiryDate,
            isPublished = a.IsPublished,
            isActive = a.IsPublished,
            a.IsFeatured,
            a.ImageUrl,
            a.AcademicYearId,
            a.CreatedBy,
            a.CreatedAt,
            a.UpdatedAt
        };
    }
}
