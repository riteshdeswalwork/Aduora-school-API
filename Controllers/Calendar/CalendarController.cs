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
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarManager _calendarManager;

        public CalendarController(ICalendarManager calendarManager)
        {
            _calendarManager = calendarManager;
        }

        [HttpPost("view")]
        public async Task<IActionResult> GetCalendarView([FromBody] CalendarFilterDTO filter)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var calendarView = await _calendarManager.GetCalendarViewAsync(userId, filter);
                return Ok(calendarView);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetCalendarItems([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var items = await _calendarManager.GetCalendarItemsAsync(userId, startDate, endDate);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("personal")]
        public async Task<IActionResult> GetPersonalCalendarItems([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var items = await _calendarManager.GetPersonalCalendarItemsAsync(userId, startDate, endDate);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("school")]
        public async Task<IActionResult> GetSchoolCalendarItems([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var items = await _calendarManager.GetSchoolCalendarItemsAsync(userId, startDate, endDate);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetCalendarSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var summary = await _calendarManager.GetCalendarSummaryAsync(userId, startDate, endDate);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("conflict-check")]
        public async Task<IActionResult> CheckConflict([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, 
            [FromQuery] Guid? excludeId = null, [FromQuery] string? itemType = null)
        {
            try
            {
                var hasConflict = await _calendarManager.HasConflictAsync(startDate, endDate, excludeId, itemType);
                return Ok(new { hasConflict, startDate, endDate });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("month/{year}/{month}")]
        public async Task<IActionResult> GetMonthView(int year, int month)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var filter = new CalendarFilterDTO
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    IncludePersonalLeaves = true
                };

                var calendarView = await _calendarManager.GetCalendarViewAsync(userId, filter);
                calendarView.ViewType = "Month";

                return Ok(calendarView);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("week/{year}/{month}/{day}")]
        public async Task<IActionResult> GetWeekView(int year, int month, int day)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var date = new DateTime(year, month, day);
                var startOfWeek = date.AddDays(-(int)date.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);

                var filter = new CalendarFilterDTO
                {
                    StartDate = startOfWeek,
                    EndDate = endOfWeek,
                    IncludePersonalLeaves = true
                };

                var calendarView = await _calendarManager.GetCalendarViewAsync(userId, filter);
                calendarView.ViewType = "Week";

                return Ok(calendarView);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("day/{year}/{month}/{day}")]
        public async Task<IActionResult> GetDayView(int year, int month, int day)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var date = new DateTime(year, month, day);

                var filter = new CalendarFilterDTO
                {
                    StartDate = date,
                    EndDate = date,
                    IncludePersonalLeaves = true
                };

                var calendarView = await _calendarManager.GetCalendarViewAsync(userId, filter);
                calendarView.ViewType = "Day";

                return Ok(calendarView);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
