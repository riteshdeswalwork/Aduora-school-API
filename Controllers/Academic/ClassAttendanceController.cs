using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Controllers.Academic
{
    [Authorize]
    public class ClassAttendanceController : ApiControllerBase
    {
        private readonly IClassAttendanceManager _classAttendanceManager;
        private readonly IClassManager _classManager;
        private readonly IClassEnrollmentManager _classEnrollmentManager;

        public ClassAttendanceController(
            IClassAttendanceManager classAttendanceManager,
            IClassManager classManager,
            IClassEnrollmentManager classEnrollmentManager)
        {
            _classAttendanceManager = classAttendanceManager;
            _classManager = classManager;
            _classEnrollmentManager = classEnrollmentManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var attendances = await _classAttendanceManager.GetAllAsync();
            var mappedAttendances = attendances.Select(MapToResponseDTO);
            return Ok(mappedAttendances);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var attendance = await _classAttendanceManager.GetClassAttendanceByIdWithRecordsAsync(id);
            if (attendance == null)
                return NotFound();

            if (User.IsInRole("Student"))
            {
                var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentIdClaim))
                    return Forbid();

                var studentId = Guid.Parse(studentIdClaim);

                var enrollment = await _classEnrollmentManager.GetEnrollmentByStudentAndClassAsync(studentId, attendance.ClassId);
                if (enrollment == null)
                    return Forbid();

                var filteredAttendance = MapToResponseDTO(attendance);
                filteredAttendance.AttendanceRecords = filteredAttendance.AttendanceRecords
                    .Where(ar => ar.StudentId == studentId)
                    .ToList();

                return Ok(filteredAttendance);
            }

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessClass(attendance.ClassId))
                return Forbid();

            return Ok(MapToResponseDTO(attendance));
        }

        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetByClass(Guid classId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            if (User.IsInRole("Student"))
            {
                var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentIdClaim))
                    return Forbid();

                var studentId = Guid.Parse(studentIdClaim);

                var enrollment = await _classEnrollmentManager.GetEnrollmentByStudentAndClassAsync(studentId, classId);
                if (enrollment == null)
                    return Forbid();
            }
            else
            {
                var scope = await GetDataScopeAsync();
                if (!scope.IsUnrestricted && !scope.CanAccessClass(classId))
                    return Forbid();
            }

            var attendances = await _classAttendanceManager.GetClassAttendancesByClassIdAsync(classId, fromDate, toDate);
            var mappedAttendances = attendances.Select(MapToResponseDTO);

            if (User.IsInRole("Student"))
            {
                var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                mappedAttendances = mappedAttendances.Select(a => {
                    a.AttendanceRecords = a.AttendanceRecords
                        .Where(ar => ar.StudentId == studentId)
                        .ToList();
                    return a;
                });
            }

            return Ok(mappedAttendances);
        }

        [HttpGet("teacher")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByTeacher([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var scope = await GetDataScopeAsync();
            if (!scope.StaffId.HasValue)
                return Forbid();

            var attendances = await _classAttendanceManager.GetClassAttendancesByTeacherIdAsync(scope.StaffId.Value, fromDate, toDate);
            if (!scope.IsUnrestricted)
            {
                var allowed = scope.AllowedClassIds?.ToHashSet() ?? new HashSet<Guid>();
                attendances = attendances.Where(a => allowed.Contains(a.ClassId));
            }

            var mappedAttendances = attendances.Select(MapToResponseDTO);
            return Ok(mappedAttendances);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(Guid studentId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            if (User.IsInRole("Student"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || studentId.ToString() != userIdClaim)
                    return Forbid();
            }
            else
            {
                var scope = await GetDataScopeAsync();
                if (!scope.IsUnrestricted && !scope.CanAccessStudent(studentId))
                    return Forbid();
            }

            var attendanceRecords = await _classAttendanceManager.GetAttendanceRecordsByStudentIdAsync(studentId, fromDate, toDate);

            var summaries = attendanceRecords
                .GroupBy(ar => ar.ClassAttendance.ClassId)
                .Select(g => new StudentAttendanceSummaryDTO
                {
                    StudentId = studentId,
                    StudentName = g.FirstOrDefault()?.Student?.User?.FullName ?? string.Empty,
                    RollNumber = g.FirstOrDefault()?.Student?.RollNumber ?? string.Empty,
                    TotalClasses = g.Count(),
                    PresentCount = g.Count(ar => ar.Status == "Present"),
                    AbsentCount = g.Count(ar => ar.Status == "Absent"),
                    LateCount = g.Count(ar => ar.Status == "Late"),
                    AttendancePercentage = g.Count() > 0
                        ? Math.Round((double)g.Count(ar => ar.Status == "Present") / g.Count() * 100, 2)
                        : 0,
                    ClassName = g.FirstOrDefault()?.ClassAttendance?.Class?.Name ?? string.Empty,
                    ClassId = g.Key
                })
                .ToList();

            return Ok(summaries);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreateClassAttendanceDTO attendanceDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessClass(attendanceDto.ClassId))
                return Forbid();

            attendanceDto.CreatedById = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            try
            {
                var attendance = await _classAttendanceManager.CreateClassAttendanceWithRecordsAsync(attendanceDto);
                return CreatedAtAction(nameof(GetById), new { id = attendance.Id }, MapToResponseDTO(attendance));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("record/{recordId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateRecord(Guid recordId, [FromBody] UpdateAttendanceRecordDTO updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record = await _classAttendanceManager.GetAttendanceRecordWithClassAsync(recordId);

            if (record == null)
                return NotFound();

            var scope = await GetDataScopeAsync();
            if (!scope.IsUnrestricted && !scope.CanAccessClass(record.ClassAttendance!.ClassId))
                return Forbid();

            var result = await _classAttendanceManager.UpdateAttendanceRecordAsync(recordId, updateDto.Status, updateDto.Remarks);
            if (!result)
                return BadRequest(new { message = "Failed to update attendance record" });

            return Ok(new { message = "Attendance record updated successfully" });
        }

        private ClassAttendanceResponseDTO MapToResponseDTO(ClassAttendance attendance)
        {
            var records = attendance.AttendanceRecords?.Select(ar => new AttendanceRecordResponseDTO
            {
                Id = ar.Id,
                StudentId = ar.StudentId,
                StudentName = ar.Student?.User?.FullName ?? string.Empty,
                StudentRollNumber = ar.Student?.RollNumber ?? string.Empty,
                Status = ar.Status,
                Remarks = ar.Remarks
            }).ToList() ?? new List<AttendanceRecordResponseDTO>();

            return new ClassAttendanceResponseDTO
            {
                Id = attendance.Id,
                ClassId = attendance.ClassId,
                ClassName = attendance.Class?.Name ?? string.Empty,
                Date = attendance.Date,
                Remarks = attendance.Remarks,
                CreatedAt = attendance.CreatedAt,
                UpdatedAt = attendance.UpdatedAt,
                CreatedById = attendance.CreatedById,
                CreatedByName = attendance.CreatedBy?.FullName ?? string.Empty,
                AttendanceRecords = records,
                TotalStudents = records.Count,
                PresentCount = records.Count(r => r.Status == "Present"),
                AbsentCount = records.Count(r => r.Status == "Absent"),
                LateCount = records.Count(r => r.Status == "Late")
            };
        }
    }
}
