using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;
using System.Security.Claims;

namespace sms.api.Controllers.Shared
{
    [Route("api/announcements")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementManager _announcementManager;

        public AnnouncementController(IAnnouncementManager announcementManager)
        {
            _announcementManager = announcementManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublishedAnnouncements()
        {
            try
            {
                var announcements = await _announcementManager.GetPublishedAsync();
                return Ok(announcements);
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
                return Ok(announcements);
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
                return Ok(announcements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnnouncementById(Guid id)
        {
            try
            {
                var announcement = await _announcementManager.GetByIdAsync(id);
                if (announcement == null)
                    return NotFound(new { message = "Announcement not found" });

                return Ok(announcement);
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
                return Ok(announcements);
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
                return Ok(announcements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAnnouncement([FromBody] Announcement announcement)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Set the creator from the current user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                announcement.CreatedBy = Guid.Parse(userId);
                var result = await _announcementManager.CreateAsync(announcement);
                return CreatedAtAction(nameof(GetAnnouncementById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAnnouncement(Guid id, [FromBody] Announcement announcement)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (id != announcement.Id)
                    return BadRequest(new { message = "ID mismatch" });

                var existingAnnouncement = await _announcementManager.GetByIdAsync(id);
                if (existingAnnouncement == null)
                    return NotFound(new { message = "Announcement not found" });

                var result = await _announcementManager.UpdateAsync(announcement);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
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
    }
}
