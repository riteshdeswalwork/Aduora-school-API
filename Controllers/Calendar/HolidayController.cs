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
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayManager _holidayManager;

        public HolidayController(IHolidayManager holidayManager)
        {
            _holidayManager = holidayManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllHolidays()
        {
            try
            {
                var holidays = await _holidayManager.GetAllHolidaysAsync();
                return Ok(holidays);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetHolidaysByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var holidays = await _holidayManager.GetHolidaysByDateRangeAsync(startDate, endDate);
                return Ok(holidays);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("my-holidays")]
        public async Task<IActionResult> GetMyHolidays([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var holidays = await _holidayManager.GetHolidaysForUserAsync(userId, startDate, endDate);
                return Ok(holidays);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHolidayById(Guid id)
        {
            try
            {
                var holiday = await _holidayManager.GetHolidayByIdAsync(id);
                if (holiday == null)
                {
                    return NotFound(new { message = "Holiday not found" });
                }
                return Ok(holiday);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayDTO createHolidayDto)
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

                var holiday = await _holidayManager.CreateHolidayAsync(createHolidayDto, userId);
                return CreatedAtAction(nameof(GetHolidayById), new { id = holiday.Id }, holiday);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHoliday(Guid id, [FromBody] UpdateHolidayDTO updateHolidayDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var holiday = await _holidayManager.UpdateHolidayAsync(id, updateHolidayDto);
                if (holiday == null)
                {
                    return NotFound(new { message = "Holiday not found" });
                }

                return Ok(holiday);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHoliday(Guid id)
        {
            try
            {
                var result = await _holidayManager.DeleteHolidayAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Holiday not found" });
                }

                return Ok(new { message = "Holiday deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveHolidays()
        {
            try
            {
                var holidays = await _holidayManager.GetActiveHolidaysAsync();
                return Ok(holidays);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("type/{holidayType}")]
        public async Task<IActionResult> GetHolidaysByType(string holidayType)
        {
            try
            {
                var holidays = await _holidayManager.GetHolidaysByTypeAsync(holidayType);
                return Ok(holidays);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("recurring")]
        public async Task<IActionResult> GetRecurringHolidays()
        {
            try
            {
                var holidays = await _holidayManager.GetRecurringHolidaysAsync();
                return Ok(holidays);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
