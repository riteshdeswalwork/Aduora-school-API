using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Controllers.Shared;
using sms.api.Interfaces;
using sms.api.Models;

namespace sms.api.Controllers.Attendance
{
    [Route("api/attendance")]
    [Authorize]
    public class AttendanceController : ApiControllerBase
    {
        private readonly IAttendanceManager _attendanceManager;

        public AttendanceController(IAttendanceManager attendanceManager)
        {
            _attendanceManager = attendanceManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var scope = await GetDataScopeAsync();
            var attendances = await _attendanceManager.GetAllAsync();
            if (!scope.IsUnrestricted)
            {
                var allowed = scope.AllowedStudentIds?.ToHashSet() ?? new HashSet<Guid>();
                attendances = attendances.Where(a => allowed.Contains(a.StudentId));
            }
            return Ok(attendances);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var attendance = await _attendanceManager.GetByIdAsync(id);
            if (attendance == null)
                return NotFound();

            if (User.IsInRole("Student") && attendance.StudentId.ToString() != User.FindFirst("nameid")?.Value)
                return Forbid();

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessStudent(attendance.StudentId))
                return Forbid();

            return Ok(attendance);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(Guid studentId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            if (User.IsInRole("Student") && studentId.ToString() != User.FindFirst("nameid")?.Value)
                return Forbid();

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessStudent(studentId))
                return Forbid();

            var attendances = await _attendanceManager.GetAttendanceByStudentIdAsync(studentId, fromDate, toDate);
            return Ok(attendances);
        }

        [HttpGet("course/{courseId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByCourse(Guid courseId, [FromQuery] DateTime? date)
        {
            var scope = await GetDataScopeAsync();
            var attendances = await _attendanceManager.GetAttendanceByCourseIdAsync(courseId, date);
            if (!scope.IsUnrestricted)
            {
                var allowed = scope.AllowedStudentIds?.ToHashSet() ?? new HashSet<Guid>();
                attendances = attendances.Where(a => allowed.Contains(a.StudentId));
            }
            return Ok(attendances);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] Models.Attendance attendance)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessStudent(attendance.StudentId))
                return Forbid();

            attendance.Id = Guid.NewGuid();
            var createdAttendance = await _attendanceManager.CreateAsync(attendance);
            return CreatedAtAction(nameof(GetById), new { id = createdAttendance.Id }, createdAttendance);
        }

        [HttpPost("mark")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceRequest request)
        {
            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessStudent(request.StudentId))
                return Forbid();

            var result = await _attendanceManager.MarkAttendanceAsync(
                request.StudentId,
                request.CourseId,
                request.Date,
                request.IsPresent,
                request.Remarks);

            if (!result)
                return BadRequest(new { message = "Failed to mark attendance" });

            return Ok(new { message = "Attendance marked successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _attendanceManager.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessStudent(existing.StudentId))
                return Forbid();

            var result = await _attendanceManager.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }

    public class MarkAttendanceRequest
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
        public string? Remarks { get; set; }
    }
}
