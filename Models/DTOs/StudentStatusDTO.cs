using System.ComponentModel.DataAnnotations;
using sms.api.Models.Enums;

namespace sms.api.Models.DTOs
{
    public class ChangeStudentStatusDTO
    {
        [Required]
        public Guid StudentId { get; set; }
        
        [Required]
        public StudentStatus NewStatus { get; set; }
        
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }
    }
    
    public class StudentStatusHistoryDTO
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public StudentStatus CurrentStatus { get; set; }
        public DateTime? StatusChangedAt { get; set; }
        public string? StatusChangeReason { get; set; }
        public Guid? StatusChangedBy { get; set; }
        public string? StatusChangedByName { get; set; }
        public string StatusDisplayName => CurrentStatus.GetDisplayName();
        public string StatusDescription => CurrentStatus.GetDescription();
        public string StatusBadgeClass => CurrentStatus.GetBadgeClass();
    }
    
    public class StudentStatusSummaryDTO
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }
        public int PassOutStudents { get; set; }
        public int LeftStudents { get; set; }
        
        public decimal ActivePercentage => TotalStudents > 0 ? (decimal)ActiveStudents / TotalStudents * 100 : 0;
        public decimal InactivePercentage => TotalStudents > 0 ? (decimal)InactiveStudents / TotalStudents * 100 : 0;
        public decimal PassOutPercentage => TotalStudents > 0 ? (decimal)PassOutStudents / TotalStudents * 100 : 0;
        public decimal LeftPercentage => TotalStudents > 0 ? (decimal)LeftStudents / TotalStudents * 100 : 0;
    }
}
