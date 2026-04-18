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

            // If staff, only allow access to their assigned classes
            if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var classInfo = await _classManager.GetClassByIdAsync(attendance.ClassId);

                if (classInfo == null || classInfo.TeacherId != staffId)
                    return Forbid();
            }
            // If student, only allow access to their own class attendance
            else if (User.IsInRole("Student"))
            {
                var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentIdClaim))
                    return Forbid();

                var studentId = Guid.Parse(studentIdClaim);

                // Check if student is enrolled in this class
                var enrollment = await _classEnrollmentManager.GetEnrollmentByStudentAndClassAsync(studentId, attendance.ClassId);
                if (enrollment == null)
                    return Forbid();

                // Filter attendance records to only show the student's own record
                var filteredAttendance = MapToResponseDTO(attendance);
                filteredAttendance.AttendanceRecords = filteredAttendance.AttendanceRecords
                    .Where(ar => ar.StudentId == studentId)
                    .ToList();

                return Ok(filteredAttendance);
            }

            return Ok(MapToResponseDTO(attendance));
        }

        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetByClass(Guid classId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            // If staff, only allow access to their assigned classes
            if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var classInfo = await _classManager.GetClassByIdAsync(classId);

                if (classInfo == null || classInfo.TeacherId != staffId)
                    return Forbid();
            }
            // If student, only allow access to their own class attendance
            else if (User.IsInRole("Student"))
            {
                var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentIdClaim))
                    return Forbid();

                var studentId = Guid.Parse(studentIdClaim);

                // Check if student is enrolled in this class
                var enrollment = await _classEnrollmentManager.GetEnrollmentByStudentAndClassAsync(studentId, classId);
                if (enrollment == null)
                    return Forbid();
            }

            var attendances = await _classAttendanceManager.GetClassAttendancesByClassIdAsync(classId, fromDate, toDate);
            var mappedAttendances = attendances.Select(MapToResponseDTO);

            // If student, filter attendance records to only show their own
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
            var teacherId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var attendances = await _classAttendanceManager.GetClassAttendancesByTeacherIdAsync(teacherId, fromDate, toDate);
            var mappedAttendances = attendances.Select(MapToResponseDTO);
            return Ok(mappedAttendances);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(Guid studentId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            // If student, only allow access to own attendance
            if (User.IsInRole("Student"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || studentId.ToString() != userIdClaim)
                    return Forbid();
            }
            // If staff, only allow access to students in their assigned classes
            else if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);

                // Get classes taught by this staff
                var classes = await _classManager.GetClassesByTeacherIdAsync(staffId);
                var classIds = classes.Select(c => c.Id).ToList();

                // Check if student is enrolled in any of these classes
                var enrollments = await _classEnrollmentManager.GetEnrollmentsByStudentIdAsync(studentId);
                var studentClassIds = enrollments.Select(e => e.ClassId).ToList();

                if (!studentClassIds.Any(id => classIds.Contains(id)))
                    return Forbid();
            }

            var attendanceRecords = await _classAttendanceManager.GetAttendanceRecordsByStudentIdAsync(studentId, fromDate, toDate);

            // Group by class and calculate summary
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

            // If staff, only allow creating attendance for their assigned classes
            if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var classInfo = await _classManager.GetClassByIdAsync(attendanceDto.ClassId);

                if (classInfo == null || classInfo.TeacherId != staffId)
                    return Forbid();
            }

            // Set the creator ID from the authenticated user
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

            // Get the record to check permissions
            var record = await _classAttendanceManager.GetAttendanceRecordWithClassAsync(recordId);

            if (record == null)
                return NotFound();

            // If staff, only allow updating records for their assigned classes
            if (User.IsInRole("Staff"))
            {
                var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffIdClaim))
                    return Forbid();

                var staffId = Guid.Parse(staffIdClaim);
                var classInfo = await _classManager.GetClassByIdAsync(record.ClassAttendance!.ClassId);

                if (classInfo == null || classInfo.TeacherId != staffId)
                    return Forbid();
            }

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
