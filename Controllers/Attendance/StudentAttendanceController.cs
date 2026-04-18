using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using sms.api.Controllers.Shared;
using sms.api.Data;
using sms.api.Models;
using AttendanceEntity = sms.api.Models.Attendance;

namespace sms.api.Controllers.Attendance
{
    [Route("api/student-attendance")]
    [Authorize(Roles = "Admin,Staff")]
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
            public Guid SubjectId { get; set; } // CourseId
            public DateTime Date { get; set; }
            public DateTime? Time { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] DateTime? date, [FromQuery] Guid? subjectId)
        {
            var q = _context.Attendances.AsNoTracking();
            if (date.HasValue) q = q.Where(a => a.Date.Date == date.Value.Date);
            if (subjectId.HasValue && subjectId.Value != Guid.Empty) q = q.Where(a => a.CourseId == subjectId.Value);
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
        public async Task<IActionResult> CheckIn([FromBody] StudentCheckDto dto)
        {
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
        public async Task<IActionResult> CheckOut([FromBody] StudentCheckDto dto)
        {
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
