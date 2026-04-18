using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace sms.api.Models.DTOs
{
    public class CreateClassAttendanceDTO
    {
        [Required]
        public Guid ClassId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string Remarks { get; set; } = string.Empty;

        [Required]
        public Guid CreatedById { get; set; }

        [Required]
        public List<AttendanceRecordDTO> AttendanceRecords { get; set; } = new List<AttendanceRecordDTO>();
    }

    public class AttendanceRecordDTO
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public string Status { get; set; } = "Present";

        public string Remarks { get; set; } = string.Empty;
    }

    public class ClassAttendanceResponseDTO
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid CreatedById { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public List<AttendanceRecordResponseDTO> AttendanceRecords { get; set; } = new List<AttendanceRecordResponseDTO>();
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
    }

    public class AttendanceRecordResponseDTO
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentRollNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Present";
        public string Remarks { get; set; } = string.Empty;
    }

    public class StudentAttendanceSummaryDTO
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double AttendancePercentage { get; set; }
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
    }

    public class UpdateAttendanceRecordDTO
    {
        [Required]
        public string Status { get; set; } = "Present";

        public string Remarks { get; set; } = string.Empty;
    }
}
