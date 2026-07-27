using System;
using System.Threading.Tasks;

namespace sms.api.Interfaces.Shared
{
    public class BiometricPunchRequest
    {
        public Guid? DeviceId { get; set; }
        public string? DeviceIdentifier { get; set; }
        public string ExternalUserId { get; set; } = string.Empty;
        public string LogType { get; set; } = "IN";
        public DateTime? LoggedAt { get; set; }
        public Guid? StaffId { get; set; }
        public Guid? StudentId { get; set; }
        public string? RawData { get; set; }
    }

    public class BiometricPunchResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? LogId { get; set; }
        public Guid? StaffId { get; set; }
        public Guid? StudentId { get; set; }
        public Guid? UserId { get; set; }
        public string? PersonName { get; set; }
        public string? PersonType { get; set; }
        public bool AttendanceApplied { get; set; }
        public string? AttendanceStatus { get; set; }
        public DateTime? AttendanceDate { get; set; }
    }

    public interface IBiometricAttendanceService
    {
        Task<BiometricPunchResult> ProcessPunchAsync(BiometricPunchRequest request, Guid? actingUserId = null);
    }
}
