using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Controllers.Shared;
using sms.api.Data;
using sms.api.Models;
using AttendanceEntity = sms.api.Models.Attendance;

namespace sms.api.Controllers.Attendance
{
    [Route("api/student-attendance")]
    [Authorize]
    public class StudentAttendanceController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        public StudentAttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class StudentCheckDto
        {
            public Guid StudentId { get; set; }
            public Guid SubjectId { get; set; }
            public DateTime Date { get; set; }
            public DateTime? Time { get; set; }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff,Teacher,Student")]
        public async Task<IActionResult> Get([FromQuery] DateTime? date, [FromQuery] Guid? subjectId)
        {
            var q = _context.Attendances.AsNoTracking();
            if (date.HasValue) q = q.Where(a => a.Date.Date == date.Value.Date);
            if (subjectId.HasValue && subjectId.Value != Guid.Empty) q = q.Where(a => a.CourseId == subjectId.Value);

            if (User.IsInRole("Student"))
            {
                // OwnOnly roles resolve AllowedStudentIds to an empty list (it's meant for
                // AssignedClasses scoping), so a student would otherwise always get zero rows.
                // Force the filter to the caller's own student id instead.
                var ownIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(ownIdClaim) || !Guid.TryParse(ownIdClaim, out var ownStudentId))
                    return Forbid();
                q = q.Where(a => a.StudentId == ownStudentId);
            }
            else
            {
                var scope = await GetDataScopeAsync();
                if (!scope.IsUnrestricted)
                {
                    var allowed = scope.AllowedStudentIds ?? Array.Empty<Guid>();
                    if (allowed.Count == 0) return Ok(Array.Empty<AttendanceEntity>());
                    q = q.Where(a => allowed.Contains(a.StudentId));
                }
            }

            var list = await q.OrderBy(a => a.Date).ThenBy(a => a.StudentId).ToListAsync();
            return Ok(list);
        }

        private async Task<Guid?> ResolveAcademicYearId()
        {
            var yearId = GetCurrentAcademicYearId();
            if (yearId == null || yearId == Guid.Empty)
            {
                var current = await _context.AcademicYears.AsNoTracking().FirstOrDefaultAsync(a => a.IsCurrentYear);
                if (current != null) yearId = current.Id;
            }
            if (yearId == null || yearId == Guid.Empty)
            {
                var latest = await _context.AcademicYears.AsNoTracking().OrderByDescending(a => a.StartDate).FirstOrDefaultAsync();
                if (latest != null) yearId = latest.Id;
            }
            return yearId;
        }

        [HttpPost("checkin")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckIn([FromBody] StudentCheckDto dto)
        {
            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessStudent(dto.StudentId))
                return Forbid();

            var yearId = await ResolveAcademicYearId();
            if (yearId == null || yearId == Guid.Empty)
                return BadRequest(new { message = "Academic year not set" });

            var now = dto.Time ?? DateTime.UtcNow;
            var existing = await _context.Attendances.FirstOrDefaultAsync(a =>
                a.StudentId == dto.StudentId && a.CourseId == dto.SubjectId && a.Date.Date == dto.Date.Date);

            if (existing == null)
            {
                existing = new AttendanceEntity
                {
                    Id = Guid.NewGuid(),
                    StudentId = dto.StudentId,
                    CourseId = dto.SubjectId,
                    AcademicYearId = yearId.Value,
                    Date = dto.Date.Date,
                    IsPresent = true,
                    CheckInTime = now,
                    Remarks = string.Empty
                };
                _context.Attendances.Add(existing);
            }
            else
            {
                existing.IsPresent = true;
                existing.CheckInTime = now;
            }
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPost("checkout")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckOut([FromBody] StudentCheckDto dto)
        {
            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessStudent(dto.StudentId))
                return Forbid();

            var now = dto.Time ?? DateTime.UtcNow;
            var existing = await _context.Attendances.FirstOrDefaultAsync(a =>
                a.StudentId == dto.StudentId && a.CourseId == dto.SubjectId && a.Date.Date == dto.Date.Date);
            if (existing == null)
                return NotFound(new { message = "Attendance not found to check out" });
            existing.CheckOutTime = now;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }
    }
}
