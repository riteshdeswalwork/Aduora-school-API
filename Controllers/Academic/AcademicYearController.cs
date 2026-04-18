using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces;
using System.Security.Claims;

namespace sms.api.Controllers.Academic
{
    [Route("api/academicyears")]
    public class AcademicYearController : ApiControllerBase
    {
        private readonly IAcademicYearManager _academicYearManager;

        public AcademicYearController(IAcademicYearManager academicYearManager)
        {
            _academicYearManager = academicYearManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAcademicYears()
        {
            try
            {
                var academicYears = await _academicYearManager.GetAllAcademicYearsAsync();
                return Ok(academicYears);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving academic years", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAcademicYearById(Guid id)
        {
            try
            {
                var academicYear = await _academicYearManager.GetAcademicYearByIdAsync(id);
                if (academicYear == null)
                    return NotFound(new { message = "Academic year not found" });

                return Ok(academicYear);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving academic year", error = ex.Message });
            }
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentAcademicYear()
        {
            try
            {
                var currentYear = await _academicYearManager.GetCurrentAcademicYearAsync();
                if (currentYear == null)
                    return NotFound(new { message = "No current academic year found" });

                return Ok(currentYear);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving current academic year", error = ex.Message });
            }
        }

        [HttpPost("create-default")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateDefaultAcademicYear()
        {
            try
            {
                // Check if any academic year exists
                var existingYears = await _academicYearManager.GetAllAcademicYearsAsync();
                if (existingYears.Any())
                {
                    return BadRequest(new { message = "Academic years already exist. Use the regular create endpoint." });
                }

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var currentYear = DateTime.Now.Year;
                var nextYear = currentYear + 1;

                var createDto = new CreateAcademicYearDTO
                {
                    YearCode = $"{currentYear}-{nextYear.ToString().Substring(2)}",
                    YearName = $"Academic Year {currentYear}-{nextYear}",
                    StartDate = new DateTime(currentYear, 4, 1), // April 1st
                    EndDate = new DateTime(nextYear, 3, 31), // March 31st
                    IsCurrentYear = true,
                    Description = "Default academic year created automatically",
                    NumberOfTerms = 2,
                    TermNames = "First Term,Second Term",
                    AdmissionStartDate = new DateTime(currentYear, 3, 1),
                    AdmissionEndDate = new DateTime(currentYear, 4, 30),
                    FinancialYear = $"{currentYear}-{nextYear.ToString().Substring(2)}"
                };

                var academicYear = await _academicYearManager.CreateAcademicYearAsync(createDto, userId);
                return Ok(new { message = "Default academic year created successfully", academicYear });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating default academic year", error = ex.Message });
            }
        }

        [HttpGet("by-code/{yearCode}")]
        public async Task<IActionResult> GetAcademicYearByCode(string yearCode)
        {
            try
            {
                var academicYear = await _academicYearManager.GetAcademicYearByCodeAsync(yearCode);
                if (academicYear == null)
                    return NotFound(new { message = "Academic year not found" });

                return Ok(academicYear);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving academic year", error = ex.Message });
            }
        }

        [HttpGet("summary/{id}")]
        public async Task<IActionResult> GetAcademicYearSummary(Guid id)
        {
            try
            {
                var summary = await _academicYearManager.GetAcademicYearSummaryAsync(id);
                return Ok(summary);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving academic year summary", error = ex.Message });
            }
        }

        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetAcademicYearsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var academicYears = await _academicYearManager.GetAcademicYearsByDateRangeAsync(startDate, endDate);
                return Ok(academicYears);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving academic years", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAcademicYear([FromBody] CreateAcademicYearDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var academicYear = await _academicYearManager.CreateAcademicYearAsync(createDto, userId);
                
                return CreatedAtAction(nameof(GetAcademicYearById), new { id = academicYear.Id }, academicYear);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating academic year", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAcademicYear(Guid id, [FromBody] UpdateAcademicYearDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var academicYear = await _academicYearManager.UpdateAcademicYearAsync(id, updateDto);
                if (academicYear == null)
                    return NotFound(new { message = "Academic year not found" });

                return Ok(academicYear);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating academic year", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAcademicYear(Guid id)
        {
            try
            {
                var result = await _academicYearManager.DeleteAcademicYearAsync(id);
                if (!result)
                    return BadRequest(new { message = "Cannot delete academic year. It may have associated data or be the current year." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting academic year", error = ex.Message });
            }
        }

        [HttpPost("{id}/set-current")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetCurrentAcademicYear(Guid id)
        {
            try
            {
                var result = await _academicYearManager.SetCurrentAcademicYearAsync(id);
                if (!result)
                    return BadRequest(new { message = "Cannot set as current academic year. Year may not exist or be inactive." });

                return Ok(new { message = "Academic year set as current successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error setting current academic year", error = ex.Message });
            }
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateAcademicYear(Guid id)
        {
            try
            {
                var result = await _academicYearManager.ActivateAcademicYearAsync(id);
                if (!result)
                    return NotFound(new { message = "Academic year not found" });

                return Ok(new { message = "Academic year activated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error activating academic year", error = ex.Message });
            }
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateAcademicYear(Guid id)
        {
            try
            {
                var result = await _academicYearManager.DeactivateAcademicYearAsync(id);
                if (!result)
                    return BadRequest(new { message = "Cannot deactivate academic year. It may not exist or be the current year." });

                return Ok(new { message = "Academic year deactivated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deactivating academic year", error = ex.Message });
            }
        }

        [HttpPost("{id}/start-new-year")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StartNewAcademicYear(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var result = await _academicYearManager.StartNewAcademicYearAsync(id, userId);
                
                if (!result)
                    return BadRequest(new { message = "Cannot start new academic year. Year may not exist or be inactive." });

                return Ok(new { message = "New academic year started successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error starting new academic year", error = ex.Message });
            }
        }

        [HttpPost("{id}/close-year")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CloseAcademicYear(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var result = await _academicYearManager.CloseAcademicYearAsync(id, userId);
                
                if (!result)
                    return NotFound(new { message = "Academic year not found" });

                return Ok(new { message = "Academic year closed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error closing academic year", error = ex.Message });
            }
        }

        [HttpGet("validate-code/{yearCode}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ValidateYearCode(string yearCode, [FromQuery] Guid? excludeId = null)
        {
            try
            {
                var isUnique = await _academicYearManager.IsAcademicYearCodeUniqueAsync(yearCode, excludeId);
                return Ok(new { isUnique });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error validating year code", error = ex.Message });
            }
        }

        [HttpGet("validate-dates")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ValidateDates([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] Guid? excludeId = null)
        {
            try
            {
                var hasOverlap = await _academicYearManager.HasOverlappingDatesAsync(startDate, endDate, excludeId);
                return Ok(new { hasOverlap });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error validating dates", error = ex.Message });
            }
        }

        [HttpGet("{id}/can-delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CanDeleteAcademicYear(Guid id)
        {
            try
            {
                var canDelete = await _academicYearManager.CanDeleteAcademicYearAsync(id);
                return Ok(new { canDelete });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error checking delete permission", error = ex.Message });
            }
        }
    }
}
