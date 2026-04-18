using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models.DTOs;
using System.Security.Claims;

namespace sms.api.Controllers.Calendar
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly IEventManager _eventManager;

        public EventController(IEventManager eventManager)
        {
            _eventManager = eventManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            try
            {
                var events = await _eventManager.GetAllEventsAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetEventsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var events = await _eventManager.GetEventsByDateRangeAsync(startDate, endDate);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("my-events")]
        public async Task<IActionResult> GetMyEvents([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var events = await _eventManager.GetEventsForUserAsync(userId, startDate, endDate);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            try
            {
                var eventItem = await _eventManager.GetEventByIdAsync(id);
                if (eventItem == null)
                {
                    return NotFound(new { message = "Event not found" });
                }
                return Ok(eventItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDTO createEventDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var eventItem = await _eventManager.CreateEventAsync(createEventDto, userId);
                return CreatedAtAction(nameof(GetEventById), new { id = eventItem.Id }, eventItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDTO updateEventDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var eventItem = await _eventManager.UpdateEventAsync(id, updateEventDto);
                if (eventItem == null)
                {
                    return NotFound(new { message = "Event not found" });
                }

                return Ok(eventItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            try
            {
                var result = await _eventManager.DeleteEventAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Event not found" });
                }

                return Ok(new { message = "Event deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveEvents()
        {
            try
            {
                var events = await _eventManager.GetActiveEventsAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("type/{eventType}")]
        public async Task<IActionResult> GetEventsByType(string eventType)
        {
            try
            {
                var events = await _eventManager.GetEventsByTypeAsync(eventType);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("priority/{priority}")]
        public async Task<IActionResult> GetEventsByPriority(string priority)
        {
            try
            {
                var events = await _eventManager.GetEventsByPriorityAsync(priority);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingEvents([FromQuery] int days = 7)
        {
            try
            {
                var events = await _eventManager.GetUpcomingEventsAsync(days);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
