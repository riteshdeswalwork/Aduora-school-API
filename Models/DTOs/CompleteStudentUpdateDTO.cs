using sms.api.Models.Enums;

namespace sms.api.Models.DTOs
{
    public class CompleteStudentUpdateDTO
    {
        // Basic Student Information
        public string? RollNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? AdmissionNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public Guid? ClassId { get; set; }
        public string? CurrentGrade { get; set; }
        public StudentStatus? Status { get; set; }
        
        // Academic Information
        public string? AcademicYear { get; set; }
        public List<Guid>? SubjectIds { get; set; }
        
        // User Information
        public UserUpdateDTO? UserInfo { get; set; }
    }

    public class UserUpdateDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }
}
