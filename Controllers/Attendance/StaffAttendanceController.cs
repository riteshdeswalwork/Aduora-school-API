using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Controllers.Shared;
using sms.api.Data;
using sms.api.Models;

namespace sms.api.Controllers.Attendance
{
    [Route("api/staff-attendance")]
    [Authorize(Roles = "Admin")]
    public class StaffAttendanceController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        public StaffAttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class StaffAttendanceRecordDto
        {
            public Guid StaffId { get; set; }
            public DateTime Date { get; set; }
            public bool IsPresent { get; set; }
            public string Remarks { get; set; } = string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] DateTime? date)
        {
            var q = _context.StaffAttendances.AsNoTracking();
            if (date.HasValue) q = q.Where(a => a.Date.Date == date.Value.Date);
            var list = await q.OrderBy(a => a.Date).ThenBy(a => a.StaffId).ToListAsync();
            return Ok(list);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> SaveBulk([FromBody] List<StaffAttendanceRecordDto> records)
        {
            if (records == null || records.Count == 0) return BadRequest(new { message = "No records provided" });
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
            if (yearId == null || yearId == Guid.Empty)
                return BadRequest(new { message = "Academic year not set" });
            foreach (var r in records)
            {
                var existing = await _context.StaffAttendances.FirstOrDefaultAsync(a => a.StaffId == r.StaffId && a.Date.Date == r.Date.Date);
                if (existing != null)
                {
                    existing.IsPresent = r.IsPresent;
                    existing.Remarks = r.Remarks ?? string.Empty;
                }
                else
                {
                    _context.StaffAttendances.Add(new StaffAttendance
                    {
                        Id = Guid.NewGuid(),
                        StaffId = r.StaffId,
                        AcademicYearId = yearId.Value,
                        Date = r.Date.Date,
                        IsPresent = r.IsPresent,
                        Remarks = r.Remarks ?? string.Empty,
                        CheckInTime = r.IsPresent ? DateTime.UtcNow : null,
                        CheckOutTime = null
                    });
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Staff attendance saved", count = records.Count });
        }

        public class StaffCheckDto
        {
            public Guid StaffId { get; set; }
            public DateTime Date { get; set; }
            public DateTime? Time { get; set; }
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] StaffCheckDto dto)
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
            if (yearId == null || yearId == Guid.Empty)
                return BadRequest(new { message = "Academic year not set" });

            var existing = await _context.StaffAttendances.FirstOrDefaultAsync(a => a.StaffId == dto.StaffId && a.Date.Date == dto.Date.Date);
            var now = dto.Time ?? DateTime.UtcNow;
            if (existing == null)
            {
                existing = new StaffAttendance
                {
                    Id = Guid.NewGuid(),
                    StaffId = dto.StaffId,
                    AcademicYearId = yearId.Value,
                    Date = dto.Date.Date,
                    IsPresent = true,
                    Remarks = string.Empty,
                    CheckInTime = now
                };
                _context.StaffAttendances.Add(existing);
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
        public async Task<IActionResult> CheckOut([FromBody] StaffCheckDto dto)
        {
            var existing = await _context.StaffAttendances.FirstOrDefaultAsync(a => a.StaffId == dto.StaffId && a.Date.Date == dto.Date.Date);
            var now = dto.Time ?? DateTime.UtcNow;
            if (existing == null)
                return NotFound(new { message = "Attendance not found to check out" });
            existing.CheckOutTime = now;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }
    }
}
