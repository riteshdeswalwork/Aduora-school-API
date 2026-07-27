using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services;

namespace sms.api.Managers.Shared
{
    public class BiometricAttendanceService : IBiometricAttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationOrchestrator? _notify;

        public BiometricAttendanceService(ApplicationDbContext context, INotificationOrchestrator? notify = null)
        {
            _context = context;
            _notify = notify;
        }

        public async Task<BiometricPunchResult> ProcessPunchAsync(BiometricPunchRequest request, Guid? actingUserId = null)
        {
            if (string.IsNullOrWhiteSpace(request.ExternalUserId) && !request.StaffId.HasValue && !request.StudentId.HasValue)
            {
                return Fail("externalUserId (fingerprint device user id) is required");
            }

            var logType = string.Equals(request.LogType, "OUT", StringComparison.OrdinalIgnoreCase) ? "OUT" : "IN";
            var loggedAt = request.LoggedAt ?? DateTime.UtcNow;
            var attendanceDate = loggedAt.Date;

            var device = await ResolveDeviceAsync(request.DeviceId, request.DeviceIdentifier);
            if (device == null && (request.DeviceId.HasValue || !string.IsNullOrWhiteSpace(request.DeviceIdentifier)))
                return Fail("Biometric device not found");

            var resolved = await ResolvePersonAsync(device?.Id, request);
            if (resolved.StaffId == null && resolved.StudentId == null)
                return Fail($"No staff/student mapped for fingerprint id '{request.ExternalUserId}'. Add a user mapping first.");

            var yearId = await ResolveAcademicYearIdAsync();
            if (yearId == null || yearId == Guid.Empty)
                return Fail("Academic year not set");

            string? attendanceStatus = null;
            var attendanceApplied = false;

            if (resolved.StaffId.HasValue)
            {
                attendanceApplied = await ApplyStaffAttendanceAsync(resolved.StaffId.Value, yearId.Value, attendanceDate, loggedAt, logType);
                attendanceStatus = logType == "IN" ? "Present (check-in)" : "Check-out";
            }
            else if (resolved.StudentId.HasValue)
            {
                attendanceApplied = await ApplyStudentAttendanceAsync(resolved.StudentId.Value, yearId.Value, attendanceDate, logType, actingUserId);
                attendanceStatus = logType == "IN" ? "Present (class)" : "Logged out";
            }

            var log = new BiometricLog
            {
                Id = Guid.NewGuid(),
                DeviceId = device?.Id,
                UserId = resolved.UserId,
                StaffId = resolved.StaffId,
                StudentId = resolved.StudentId,
                LoggedAt = loggedAt,
                LogType = logType,
                ExternalUserId = request.ExternalUserId?.Trim(),
                RawData = Truncate(request.RawData ?? $"fingerprint match → attendance {attendanceDate:yyyy-MM-dd}", 500),
                AttendanceApplied = attendanceApplied,
                IsActive = true
            };
            _context.BiometricLogs.Add(log);
            await _context.SaveChangesAsync();

            if (resolved.UserId.HasValue && resolved.UserId != Guid.Empty)
            {
                await NotificationEvents.NotifyUsersAsync(
                    _notify,
                    new[] { resolved.UserId.Value },
                    "Biometric attendance recorded",
                    $"{resolved.PersonType} check-{logType.ToLower()} recorded for {attendanceDate:dd MMM yyyy}.",
                    "attendance.biometric",
                    "attendance",
                    NotificationChannels.Push | NotificationChannels.InApp,
                    log.Id,
                    "BiometricLog",
                    idempotencyKey: $"biometric.{log.Id}");
            }

            return new BiometricPunchResult
            {
                Success = true,
                Message = attendanceApplied
                    ? $"Fingerprint matched. {resolved.PersonType} attendance marked for {attendanceDate:yyyy-MM-dd}."
                    : $"Fingerprint matched ({resolved.PersonName}), but attendance was not updated.",
                LogId = log.Id,
                StaffId = resolved.StaffId,
                StudentId = resolved.StudentId,
                UserId = resolved.UserId,
                PersonName = resolved.PersonName,
                PersonType = resolved.PersonType,
                AttendanceApplied = attendanceApplied,
                AttendanceStatus = attendanceStatus,
                AttendanceDate = attendanceDate
            };
        }

        private async Task<BiometricDevice?> ResolveDeviceAsync(Guid? deviceId, string? deviceIdentifier)
        {
            if (deviceId.HasValue && deviceId != Guid.Empty)
                return await _context.BiometricDevices.FirstOrDefaultAsync(d => d.Id == deviceId.Value && d.IsActive);

            if (!string.IsNullOrWhiteSpace(deviceIdentifier))
            {
                var id = deviceIdentifier.Trim();
                return await _context.BiometricDevices.FirstOrDefaultAsync(d =>
                    d.IsActive && d.DeviceIdentifier == id);
            }

            return null;
        }

        private sealed class ResolvedPerson
        {
            public Guid? StaffId { get; set; }
            public Guid? StudentId { get; set; }
            public Guid? UserId { get; set; }
            public string? PersonName { get; set; }
            public string? PersonType { get; set; }
        }

        private async Task<ResolvedPerson> ResolvePersonAsync(Guid? deviceId, BiometricPunchRequest request)
        {
            var result = new ResolvedPerson();

            if (request.StaffId.HasValue && request.StaffId != Guid.Empty)
            {
                var staff = await _context.Staff.AsNoTracking().Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == request.StaffId.Value);
                if (staff != null)
                {
                    result.StaffId = staff.Id;
                    result.UserId = staff.UserId;
                    result.PersonName = NameOf(staff.User);
                    result.PersonType = "Staff";
                    return result;
                }
            }

            if (request.StudentId.HasValue && request.StudentId != Guid.Empty)
            {
                var student = await _context.Students.AsNoTracking().Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == request.StudentId.Value);
                if (student != null)
                {
                    result.StudentId = student.Id;
                    result.UserId = student.UserId;
                    result.PersonName = NameOf(student.User);
                    result.PersonType = "Student";
                    return result;
                }
            }

            var externalId = (request.ExternalUserId ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(externalId))
                return result;

            var mapQuery = _context.BiometricUserMaps.AsNoTracking()
                .Where(m => m.IsActive && m.ExternalUserId == externalId);
            if (deviceId.HasValue)
                mapQuery = mapQuery.Where(m => m.DeviceId == null || m.DeviceId == deviceId.Value);

            var map = await mapQuery.OrderByDescending(m => m.DeviceId != null).FirstOrDefaultAsync();
            if (map != null)
            {
                if (map.StaffId.HasValue)
                {
                    var staff = await _context.Staff.AsNoTracking().Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.Id == map.StaffId.Value);
                    if (staff != null)
                    {
                        result.StaffId = staff.Id;
                        result.UserId = staff.UserId;
                        result.PersonName = map.DisplayName ?? NameOf(staff.User);
                        result.PersonType = "Staff";
                        return result;
                    }
                }
                if (map.StudentId.HasValue)
                {
                    var student = await _context.Students.AsNoTracking().Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.Id == map.StudentId.Value);
                    if (student != null)
                    {
                        result.StudentId = student.Id;
                        result.UserId = student.UserId;
                        result.PersonName = map.DisplayName ?? NameOf(student.User);
                        result.PersonType = "Student";
                        return result;
                    }
                }
            }

            // Fallback: EmployeeId / RegistrationNumber == device user id
            var staffByEmp = await _context.Staff.AsNoTracking().Include(s => s.User)
                .FirstOrDefaultAsync(s => s.EmployeeId != null && s.EmployeeId == externalId);
            if (staffByEmp != null)
            {
                result.StaffId = staffByEmp.Id;
                result.UserId = staffByEmp.UserId;
                result.PersonName = NameOf(staffByEmp.User);
                result.PersonType = "Staff";
                return result;
            }

            var studentByReg = await _context.Students.AsNoTracking().Include(s => s.User)
                .FirstOrDefaultAsync(s =>
                    (s.RegistrationNumber != null && s.RegistrationNumber == externalId) ||
                    (s.AdmissionNumber != null && s.AdmissionNumber == externalId) ||
                    (s.RollNumber != null && s.RollNumber == externalId));
            if (studentByReg != null)
            {
                result.StudentId = studentByReg.Id;
                result.UserId = studentByReg.UserId;
                result.PersonName = NameOf(studentByReg.User);
                result.PersonType = "Student";
            }

            return result;
        }

        private async Task<bool> ApplyStaffAttendanceAsync(Guid staffId, Guid yearId, DateTime date, DateTime loggedAt, string logType)
        {
            var existing = await _context.StaffAttendances
                .FirstOrDefaultAsync(a => a.StaffId == staffId && a.Date.Date == date);

            if (logType == "OUT")
            {
                if (existing == null)
                {
                    // OUT without IN — still create Present record with check-out
                    existing = new StaffAttendance
                    {
                        Id = Guid.NewGuid(),
                        StaffId = staffId,
                        AcademicYearId = yearId,
                        Date = date,
                        Status = "Present",
                        IsPresent = true,
                        Remarks = "Biometric check-out",
                        CheckInTime = loggedAt,
                        CheckOutTime = loggedAt
                    };
                    _context.StaffAttendances.Add(existing);
                }
                else
                {
                    existing.CheckOutTime = loggedAt;
                    existing.Status = "Present";
                    existing.IsPresent = true;
                    if (string.IsNullOrWhiteSpace(existing.Remarks))
                        existing.Remarks = "Biometric check-out";
                }
                return true;
            }

            // IN
            if (existing == null)
            {
                _context.StaffAttendances.Add(new StaffAttendance
                {
                    Id = Guid.NewGuid(),
                    StaffId = staffId,
                    AcademicYearId = yearId,
                    Date = date,
                    Status = "Present",
                    IsPresent = true,
                    Remarks = "Biometric fingerprint",
                    CheckInTime = loggedAt
                });
            }
            else
            {
                existing.Status = "Present";
                existing.IsPresent = true;
                existing.CheckInTime ??= loggedAt;
                if (string.IsNullOrWhiteSpace(existing.Remarks) || existing.Remarks == "Absent")
                    existing.Remarks = "Biometric fingerprint";
            }
            return true;
        }

        private async Task<bool> ApplyStudentAttendanceAsync(Guid studentId, Guid yearId, DateTime date, string logType, Guid? actingUserId)
        {
            if (logType == "OUT")
                return true; // log only for students (class record is day present)

            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null) return false;

            Guid? classId = student.ClassId;
            if (classId == null || classId == Guid.Empty)
            {
                classId = await _context.ClassEnrollments.AsNoTracking()
                    .Where(e => e.StudentId == studentId && e.IsActive && e.AcademicYearId == yearId)
                    .Select(e => (Guid?)e.ClassId)
                    .FirstOrDefaultAsync();
            }
            if (classId == null || classId == Guid.Empty)
                return false;

            var session = await _context.ClassAttendances
                .FirstOrDefaultAsync(c => c.ClassId == classId.Value && c.Date.Date == date && c.AcademicYearId == yearId);

            if (session == null)
            {
                var createdBy = actingUserId;
                if (createdBy == null || createdBy == Guid.Empty)
                {
                    createdBy = await _context.Users.AsNoTracking()
                        .Where(u => u.Role == "Admin" && u.IsActive)
                        .Select(u => (Guid?)u.Id)
                        .FirstOrDefaultAsync();
                }
                if (createdBy == null || createdBy == Guid.Empty)
                    return false;

                session = new ClassAttendance
                {
                    Id = Guid.NewGuid(),
                    ClassId = classId.Value,
                    AcademicYearId = yearId,
                    Date = date,
                    Remarks = "Biometric gate attendance",
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = createdBy.Value
                };
                _context.ClassAttendances.Add(session);
                await _context.SaveChangesAsync();
            }

            var record = await _context.ClassAttendanceRecords
                .FirstOrDefaultAsync(r => r.ClassAttendanceId == session.Id && r.StudentId == studentId);

            if (record == null)
            {
                _context.ClassAttendanceRecords.Add(new ClassAttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    ClassAttendanceId = session.Id,
                    StudentId = studentId,
                    AcademicYearId = yearId,
                    Status = "Present",
                    IsPresent = true,
                    Remarks = "Biometric fingerprint",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                record.Status = "Present";
                record.IsPresent = true;
                record.Remarks = "Biometric fingerprint";
                record.UpdatedAt = DateTime.UtcNow;
                record.IsActive = true;
            }

            return true;
        }

        private async Task<Guid?> ResolveAcademicYearIdAsync()
        {
            var current = await _context.AcademicYears.AsNoTracking().FirstOrDefaultAsync(a => a.IsCurrentYear);
            if (current != null) return current.Id;
            var latest = await _context.AcademicYears.AsNoTracking().OrderByDescending(a => a.StartDate).FirstOrDefaultAsync();
            return latest?.Id;
        }

        private static string NameOf(User? user)
        {
            if (user == null) return "Unknown";
            var name = $"{user.FirstName} {user.LastName}".Trim();
            return string.IsNullOrEmpty(name) ? (user.Username ?? "Unknown") : name;
        }

        private static string Truncate(string value, int max) =>
            value.Length <= max ? value : value[..max];

        private static BiometricPunchResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
