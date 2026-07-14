using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models.DTOs;
using System.Security.Claims;

namespace sms.api.Controllers.Academic
{
    [Route("api/[controller]")]
    [Authorize]
    public class TimetableController : ApiControllerBase
    {
        private readonly ITimetableManager _timetableManager;

        public TimetableController(ITimetableManager timetableManager)
        {
            _timetableManager = timetableManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllTimetableEntries([FromQuery] DateTime? scheduleDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var scope = await GetDataScopeAsync();
                var entries = await _timetableManager.GetAllTimetableEntriesAsync(scheduleDate, endDate);
                if (!scope.IsUnrestricted)
                {
                    var allowed = scope.AllowedClassIds?.ToHashSet() ?? new HashSet<Guid>();
                    entries = entries.Where(e => allowed.Contains(e.ClassId));
                }
                return Ok(entries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetTimetableByClass(Guid classId, [FromQuery] int? dayOfWeek = null, [FromQuery] DateTime? scheduleDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                if (userRole != "Student")
                {
                    var scope = await GetDataScopeAsync();
                    if (!scope.IsUnrestricted && !scope.CanAccessClass(classId))
                        return Forbid();
                }

                var entries = await _timetableManager.GetTimetableByClassAsync(classId, dayOfWeek, scheduleDate, endDate);
                return Ok(entries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("teacher/{teacherId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetTimetableByTeacher(Guid teacherId, [FromQuery] int? dayOfWeek = null, [FromQuery] DateTime? scheduleDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var scope = await GetDataScopeAsync();
                var entries = await _timetableManager.GetTimetableByTeacherAsync(teacherId, dayOfWeek, scheduleDate, endDate);
                if (!scope.IsUnrestricted)
                {
                    var allowed = scope.AllowedClassIds?.ToHashSet() ?? new HashSet<Guid>();
                    entries = entries.Where(e => allowed.Contains(e.ClassId));
                }
                return Ok(entries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTimetableEntryById(Guid id)
        {
            try
            {
                var entry = await _timetableManager.GetTimetableEntryByIdAsync(id);
                if (entry == null)
                {
                    return NotFound(new { message = "Timetable entry not found" });
                }

                if (!User.IsInRole("Student"))
                {
                    var scope = await GetDataScopeAsync();
                    if (!scope.IsUnrestricted && !scope.CanAccessClass(entry.ClassId))
                        return Forbid();
                }

                return Ok(entry);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTimetableEntry([FromBody] CreateTimetableEntryDTO createTimetableEntryDto)
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

                if (!createTimetableEntryDto.ScheduleDate.HasValue)
                    return BadRequest(new { message = "ScheduleDate is required. Timetable is stored per calendar day only." });

                var firstPeriodError = await _timetableManager.ValidateClassTeacherForFirstPeriodAsync(
                    createTimetableEntryDto.PeriodId,
                    createTimetableEntryDto.ClassId,
                    createTimetableEntryDto.TeacherId,
                    createTimetableEntryDto.AcademicYearId);
                if (firstPeriodError != null)
                {
                    return BadRequest(new { message = firstPeriodError });
                }

                // Same teacher cannot teach two overlapping slots on the same weekday (this academic year).
                if (createTimetableEntryDto.TeacherId.HasValue)
                {
                    var conflictCheck = new TeacherConflictCheckDTO
                    {
                        TeacherId = createTimetableEntryDto.TeacherId.Value,
                        PeriodId = createTimetableEntryDto.PeriodId,
                        AcademicYearId = createTimetableEntryDto.AcademicYearId,
                        ScheduleDateForConflict = createTimetableEntryDto.ScheduleDate?.Date
                    };

                    var (hasConflict, blocking) = await _timetableManager.CheckTeacherConflictWithSummaryAsync(conflictCheck);
                    if (hasConflict)
                    {
                        var msg = string.IsNullOrEmpty(blocking)
                            ? "This teacher is already assigned to another class in an overlapping time slot on this day."
                            : $"This teacher is already assigned on this date: {blocking}. Two overlapping periods cannot use the same teacher.";
                        return BadRequest(new { message = msg });
                    }
                }

                var entry = await _timetableManager.CreateTimetableEntryAsync(createTimetableEntryDto, userId);
                return CreatedAtAction(nameof(GetTimetableEntryById), new { id = entry.Id }, entry);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message;
                var msg = string.IsNullOrEmpty(inner) ? ex.Message : $"{ex.Message} {inner}";
                return StatusCode(500, new { error = $"An error occurred: {msg}" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTimetableEntry(Guid id, [FromBody] UpdateTimetableEntryDTO updateTimetableEntryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingEntry = await _timetableManager.GetTimetableEntryByIdAsync(id);
                if (existingEntry == null)
                {
                    return NotFound(new { message = "Timetable entry not found" });
                }

                var effectiveTeacherId = updateTimetableEntryDto.TeacherId ?? existingEntry.TeacherId;

                var firstPeriodError = await _timetableManager.ValidateClassTeacherForFirstPeriodAsync(
                    existingEntry.PeriodId,
                    existingEntry.ClassId,
                    effectiveTeacherId,
                    existingEntry.AcademicYearId);
                if (firstPeriodError != null)
                {
                    return BadRequest(new { message = firstPeriodError });
                }

                if (updateTimetableEntryDto.ClearScheduleDate)
                    return BadRequest(new { message = "Schedule date cannot be removed. Every entry stays on a calendar day." });

                if (effectiveTeacherId.HasValue)
                {
                    DateTime? evalDate = updateTimetableEntryDto.ScheduleDate?.Date ?? existingEntry.ScheduleDate?.Date;

                    var conflictCheck = new TeacherConflictCheckDTO
                    {
                        TeacherId = effectiveTeacherId.Value,
                        PeriodId = existingEntry.PeriodId,
                        ExcludeTimetableEntryId = id,
                        AcademicYearId = existingEntry.AcademicYearId,
                        ScheduleDateForConflict = evalDate
                    };

                    var (hasConflict, blocking) = await _timetableManager.CheckTeacherConflictWithSummaryAsync(conflictCheck);
                    if (hasConflict)
                    {
                        var msg = string.IsNullOrEmpty(blocking)
                            ? "This teacher is already assigned to another class in an overlapping time slot on this day."
                            : $"This teacher is already assigned on this date: {blocking}. Two overlapping periods cannot use the same teacher.";
                        return BadRequest(new { message = msg });
                    }
                }

                var entry = await _timetableManager.UpdateTimetableEntryAsync(id, updateTimetableEntryDto);
                if (entry == null)
                {
                    return NotFound(new { message = "Timetable entry not found" });
                }

                return Ok(entry);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTimetableEntry(Guid id)
        {
            try
            {
                var result = await _timetableManager.DeleteTimetableEntryAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Timetable entry not found" });
                }

                return Ok(new { message = "Timetable entry deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("view/class/{classId}")]
        public async Task<IActionResult> GetClassTimetableView(Guid classId)
        {
            try
            {
                if (!User.IsInRole("Student"))
                {
                    var scope = await GetDataScopeAsync();
                    if (!scope.IsUnrestricted && !scope.CanAccessClass(classId))
                        return Forbid();
                }

                var timetable = await _timetableManager.GetClassTimetableAsync(classId);
                return Ok(timetable);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("view/weekly")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetWeeklyTimetable()
        {
            try
            {
                var timetable = await _timetableManager.GetWeeklyTimetableAsync();
                return Ok(timetable);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("view/my-timetable")]
        public async Task<IActionResult> GetMyTimetable()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var timetable = await _timetableManager.GetWeeklyTimetableForUserAsync(userId);
                return Ok(timetable);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("check-teacher-conflict")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckTeacherConflict([FromBody] TeacherConflictCheckDTO conflictCheck)
        {
            try
            {
                var hasConflict = await _timetableManager.HasTeacherConflictAsync(conflictCheck);
                return Ok(new { hasConflict });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("conflicts/teacher/{teacherId}/period/{periodId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTeacherConflicts(Guid teacherId, Guid periodId, [FromQuery] DateTime? specificDate = null)
        {
            try
            {
                var conflicts = await _timetableManager.GetTeacherConflictsAsync(teacherId, periodId, specificDate);
                return Ok(conflicts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("assign-substitute")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignSubstituteTeacher([FromBody] AssignSubstituteDTO assignSubstituteDto)
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

                var result = await _timetableManager.AssignSubstituteTeacherAsync(assignSubstituteDto, userId);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to assign substitute teacher. Teacher may have a conflict." });
                }

                return Ok(new { message = "Substitute teacher assigned successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("substitute/{timetableEntryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveSubstituteTeacher(Guid timetableEntryId, [FromQuery] DateTime substituteDate)
        {
            try
            {
                var result = await _timetableManager.RemoveSubstituteTeacherAsync(timetableEntryId, substituteDate);
                if (!result)
                {
                    return NotFound(new { message = "Substitute assignment not found" });
                }

                return Ok(new { message = "Substitute teacher removed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("substitutes/{date}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetSubstituteAssignments(DateTime date)
        {
            try
            {
                var assignments = await _timetableManager.GetSubstituteAssignmentsAsync(date);
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("substitute-history/teacher/{teacherId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetTeacherSubstituteHistory(Guid teacherId)
        {
            try
            {
                var history = await _timetableManager.GetTeacherSubstituteHistoryAsync(teacherId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("bulk/class/{classId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateClassTimetable(Guid classId, [FromBody] List<CreateTimetableEntryDTO> entries)
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

                var result = await _timetableManager.CreateClassTimetableAsync(classId, entries, userId);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to create class timetable" });
                }

                return Ok(new { message = "Class timetable created successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("copy/{fromClassId}/to/{toClassId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CopyTimetable(Guid fromClassId, Guid toClassId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _timetableManager.CopyTimetableAsync(fromClassId, toClassId, userId);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to copy timetable. Source class may not have a timetable." });
                }

                return Ok(new { message = "Timetable copied successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>Copy one calendar day&apos;s visible timetable to another date (new dated rows).</summary>
        [HttpPost("copy-day")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CopyTimetableDay([FromBody] CopyTimetableDayDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { message = "Invalid user token" });

                if (dto.FromDate.Date == dto.ToDate.Date)
                {
                    return BadRequest(new
                    {
                        message = "Source date and target date must be different."
                    });
                }

                var targetErr = await _timetableManager.ValidateCopyDayTargetDateAsync(dto.AcademicYearId, dto.ToDate);
                if (targetErr != null)
                    return BadRequest(new { message = targetErr });

                var count = await _timetableManager.CopyTimetableDayAsync(dto, userId);
                return Ok(new { message = $"Copied {count} timetable row(s).", count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}