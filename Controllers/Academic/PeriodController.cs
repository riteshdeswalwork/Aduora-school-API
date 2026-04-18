using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models.DTOs;
using System.Security.Claims;

namespace sms.api.Controllers.Academic
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PeriodController : ControllerBase
    {
        private readonly IPeriodManager _periodManager;

        public PeriodController(IPeriodManager periodManager)
        {
            _periodManager = periodManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPeriods([FromQuery] Guid? academicYearId = null)
        {
            try
            {
                var periods = await _periodManager.GetAllPeriodsAsync(academicYearId);
                return Ok(periods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("day/{dayOfWeek}")]
        public async Task<IActionResult> GetPeriodsByDay(int dayOfWeek, [FromQuery] Guid? academicYearId = null)
        {
            try
            {
                if (dayOfWeek < 1 || dayOfWeek > 7)
                {
                    return BadRequest(new { message = "Day of week must be between 1 (Monday) and 7 (Sunday)" });
                }

                var periods = await _periodManager.GetPeriodsByDayAsync(dayOfWeek, academicYearId);
                return Ok(periods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetPeriodsForClass(Guid classId, [FromQuery] int? dayOfWeek = null, [FromQuery] Guid? academicYearId = null)
        {
            try
            {
                if (dayOfWeek.HasValue && (dayOfWeek < 1 || dayOfWeek > 7))
                {
                    return BadRequest(new { message = "Day of week must be between 1 (Monday) and 7 (Sunday)" });
                }

                var periods = await _periodManager.GetPeriodsForClassAsync(classId, dayOfWeek, academicYearId);
                return Ok(periods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPeriodById(Guid id)
        {
            try
            {
                var period = await _periodManager.GetPeriodByIdAsync(id);
                if (period == null)
                {
                    return NotFound(new { message = "Period not found" });
                }
                return Ok(period);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePeriod([FromBody] CreatePeriodDTO createPeriodDto)
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

                // Check for time conflicts (within same academic year)
                var hasConflict = await _periodManager.HasTimeConflictAsync(
                    createPeriodDto.StartTime, 
                    createPeriodDto.EndTime, 
                    createPeriodDto.DayOfWeek,
                    excludePeriodId: null,
                    academicYearId: createPeriodDto.AcademicYearId);

                if (hasConflict)
                {
                    return BadRequest(new { message = "Time conflict with existing period" });
                }

                var period = await _periodManager.CreatePeriodAsync(createPeriodDto, userId);
                return CreatedAtAction(nameof(GetPeriodById), new { id = period.Id }, period);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePeriod(Guid id, [FromBody] UpdatePeriodDTO updatePeriodDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check for time conflicts if time is being updated (within same academic year)
                if (updatePeriodDto.StartTime.HasValue && updatePeriodDto.EndTime.HasValue && updatePeriodDto.DayOfWeek.HasValue)
                {
                    var academicYearId = updatePeriodDto.AcademicYearId ?? (await _periodManager.GetPeriodByIdAsync(id))?.AcademicYearId;
                    var hasConflict = await _periodManager.HasTimeConflictAsync(
                        updatePeriodDto.StartTime.Value, 
                        updatePeriodDto.EndTime.Value, 
                        updatePeriodDto.DayOfWeek.Value,
                        excludePeriodId: id,
                        academicYearId: academicYearId);

                    if (hasConflict)
                    {
                        return BadRequest(new { message = "Time conflict with existing period" });
                    }
                }

                var period = await _periodManager.UpdatePeriodAsync(id, updatePeriodDto);
                if (period == null)
                {
                    return NotFound(new { message = "Period not found" });
                }

                return Ok(period);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePeriod(Guid id)
        {
            try
            {
                var result = await _periodManager.DeletePeriodAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Period not found" });
                }

                return Ok(new { message = "Period deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActivePeriods([FromQuery] Guid? academicYearId = null)
        {
            try
            {
                var periods = await _periodManager.GetActivePeriodsAsync(academicYearId);
                return Ok(periods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("type/{periodType}")]
        public async Task<IActionResult> GetPeriodsByType(string periodType, [FromQuery] Guid? academicYearId = null)
        {
            try
            {
                var periods = await _periodManager.GetPeriodsByTypeAsync(periodType, academicYearId);
                return Ok(periods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("ordered/{dayOfWeek}")]
        public async Task<IActionResult> GetOrderedPeriods(int dayOfWeek, [FromQuery] Guid? academicYearId = null)
        {
            try
            {
                if (dayOfWeek < 1 || dayOfWeek > 7)
                {
                    return BadRequest(new { message = "Day of week must be between 1 (Monday) and 7 (Sunday)" });
                }

                var periods = await _periodManager.GetOrderedPeriodsAsync(dayOfWeek, academicYearId);
                return Ok(periods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("check-conflict")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckTimeConflict([FromBody] dynamic conflictData)
        {
            try
            {
                var startTime = TimeSpan.Parse(conflictData.startTime.ToString());
                var endTime = TimeSpan.Parse(conflictData.endTime.ToString());
                var dayOfWeek = (int)conflictData.dayOfWeek;
                Guid? excludePeriodId = conflictData.excludePeriodId != null ? Guid.Parse(conflictData.excludePeriodId.ToString()) : null;

                var hasConflict = await _periodManager.HasTimeConflictAsync(startTime, endTime, dayOfWeek, excludePeriodId);
                return Ok(new { hasConflict });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
