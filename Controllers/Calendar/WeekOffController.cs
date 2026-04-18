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
    public class WeekOffController : ControllerBase
    {
        private readonly IWeekOffManager _weekOffManager;

        public WeekOffController(IWeekOffManager weekOffManager)
        {
            _weekOffManager = weekOffManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWeekOffs()
        {
            try
            {
                var weekOffs = await _weekOffManager.GetAllWeekOffsAsync();
                return Ok(weekOffs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("my-weekoffs")]
        public async Task<IActionResult> GetMyWeekOffs()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var weekOffs = await _weekOffManager.GetWeekOffsForUserAsync(userId);
                return Ok(weekOffs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWeekOffById(Guid id)
        {
            try
            {
                var weekOff = await _weekOffManager.GetWeekOffByIdAsync(id);
                if (weekOff == null)
                {
                    return NotFound(new { message = "Week off not found" });
                }
                return Ok(weekOff);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateWeekOff([FromBody] CreateWeekOffDTO createWeekOffDto)
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

                var weekOff = await _weekOffManager.CreateWeekOffAsync(createWeekOffDto, userId);
                return CreatedAtAction(nameof(GetWeekOffById), new { id = weekOff.Id }, weekOff);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateWeekOff(Guid id, [FromBody] UpdateWeekOffDTO updateWeekOffDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var weekOff = await _weekOffManager.UpdateWeekOffAsync(id, updateWeekOffDto);
                if (weekOff == null)
                {
                    return NotFound(new { message = "Week off not found" });
                }

                return Ok(weekOff);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteWeekOff(Guid id)
        {
            try
            {
                var result = await _weekOffManager.DeleteWeekOffAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Week off not found" });
                }

                return Ok(new { message = "Week off deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveWeekOffs()
        {
            try
            {
                var weekOffs = await _weekOffManager.GetActiveWeekOffsAsync();
                return Ok(weekOffs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("day/{dayOfWeek}")]
        public async Task<IActionResult> GetWeekOffsByDay(int dayOfWeek)
        {
            try
            {
                if (dayOfWeek < 0 || dayOfWeek > 6)
                {
                    return BadRequest(new { message = "Day of week must be between 0 (Sunday) and 6 (Saturday)" });
                }

                var weekOffs = await _weekOffManager.GetWeekOffsByDayAsync(dayOfWeek);
                return Ok(weekOffs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("check/{date}")]
        public async Task<IActionResult> IsWeekOff(DateTime date)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid? userId = null;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }

                var isWeekOff = await _weekOffManager.IsWeekOffAsync(date, userId);
                return Ok(new { date = date.ToString("yyyy-MM-dd"), isWeekOff });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
