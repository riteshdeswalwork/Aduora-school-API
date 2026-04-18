using System;
using System.ComponentModel.DataAnnotations;
using sms.api.Models.Enums;
using System.Text.Json.Serialization;

namespace sms.api.Models.DTOs
{
    public class CreateStudentDTO
    {
        // 👤 Personal Details
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Father's name is required")]
        public string FatherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mother's name is required")]
        public string MotherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Blood group is required")]
        public string BloodGroup { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aadhaar number is required")]
        public string AadhaarNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Religion is required")]
        public string Religion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty; // SC/ST/OBC/General

        // 🏠 Contact & Address Details
        [Required(ErrorMessage = "Permanent address is required")]
        public string PermanentAddress { get; set; } = string.Empty;

        public string CurrentAddress { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        // 🏫 School Related Details
        [Required(ErrorMessage = "Admission number is required")]
        public string AdmissionNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of admission is required")]
        public DateTime DateOfAdmission { get; set; }

        public Guid? ClassId { get; set; }

        // 🚌 Bus Transportation
        public Guid? BusStopId { get; set; }

        [Required(ErrorMessage = "Roll number is required")]
        public string RollNumber { get; set; } = string.Empty;

        public string PreviousSchoolName { get; set; } = string.Empty;

        // 🧑‍🏫 Parent/Guardian Details
        [Required(ErrorMessage = "Guardian name is required")]
        public string GuardianName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Guardian contact is required")]
        public string GuardianContact { get; set; } = string.Empty;

        public decimal AnnualIncome { get; set; }

        // Login credentials
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        // Subject selection
        public List<Guid> SelectedSubjectIds { get; set; } = new();

        // Optional subjects (subjects marked as optional by student)
        public List<Guid> OptionalSubjectIds { get; set; } = new();

        // Documents
        public List<CreateDocumentDTO> Documents { get; set; } = new();

        // Auto-calculated monthly fee (readonly)
        public decimal MonthlyFee { get; set; }

        // Student Status
        public StudentStatus Status { get; set; } = StudentStatus.Active;

        // Profile photo as base64 string
        public string? ProfilePhotoData { get; set; }

        /// <summary>Optional. When provided, assigns this role to the user and creates UserRole.</summary>
        public Guid? RoleId { get; set; }
    }

    public class CreateDocumentDTO
    {
        public string DocumentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] DocumentData { get; set; } = Array.Empty<byte>();
    }

    // Simple DTO for student list display
    public class StudentListDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public StudentStatus Status { get; set; } = StudentStatus.Active;
        public bool IsActive => Status == StudentStatus.Active;

        [JsonPropertyName("profilePicture")]
        public string? ProfilePicture { get; set; } = null;
    }
}
