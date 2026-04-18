using System;
using System.ComponentModel.DataAnnotations;

namespace sms.api.DTOs
{
    public class ClassDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Class name is required")]
        [StringLength(50, ErrorMessage = "Class name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grade level is required")]
        [StringLength(20, ErrorMessage = "Grade level cannot exceed 20 characters")]
        public string GradeLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Academic year is required")]
        [StringLength(20, ErrorMessage = "Academic year cannot exceed 20 characters")]
        public string AcademicYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section is required")]
        [StringLength(10, ErrorMessage = "Section cannot exceed 10 characters")]
        public string Section { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room number is required")]
        [StringLength(10, ErrorMessage = "Room number cannot exceed 10 characters")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maximum capacity is required")]
        [Range(1, 100, ErrorMessage = "Maximum capacity must be between 1 and 100")]
        public int MaxCapacity { get; set; }

        public int CurrentEnrollment { get; set; }

        public Guid? TeacherId { get; set; }
        public string? TeacherName { get; set; }

        public decimal MonthlyFee { get; set; }
        public decimal YearlyFee { get; set; }
        public string FeeType { get; set; } = "Monthly";

        [Range(1, 20, ErrorMessage = "Minimum subjects must be between 1 and 20")]
        public int MinimumSubjects { get; set; } = 5;

        public List<Guid> SubjectIds { get; set; } = new();
        public List<SubjectDTO> Subjects { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateClassDTO
    {
        [Required(ErrorMessage = "Class name is required")]
        [StringLength(50, ErrorMessage = "Class name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grade level is required")]
        [StringLength(20, ErrorMessage = "Grade level cannot exceed 20 characters")]
        public string GradeLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Academic year is required")]
        [StringLength(20, ErrorMessage = "Academic year cannot exceed 20 characters")]
        public string AcademicYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section is required")]
        [StringLength(10, ErrorMessage = "Section cannot exceed 10 characters")]
        public string Section { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room number is required")]
        [StringLength(10, ErrorMessage = "Room number cannot exceed 10 characters")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maximum capacity is required")]
        [Range(1, 100, ErrorMessage = "Maximum capacity must be between 1 and 100")]
        public int MaxCapacity { get; set; }

        public Guid? TeacherId { get; set; }

        public decimal MonthlyFee { get; set; }
        public decimal YearlyFee { get; set; }
        public string FeeType { get; set; } = "Monthly";

        [Range(1, 20, ErrorMessage = "Minimum subjects must be between 1 and 20")]
        public int MinimumSubjects { get; set; } = 5;

        /// <summary>Optional. When provided, used for ClassSubjects to satisfy FK.</summary>
        public Guid? AcademicYearId { get; set; }

        public List<Guid> SubjectIds { get; set; } = new();
        public List<Guid> CompulsorySubjectIds { get; set; } = new();
        public List<ClassSubjectDTO> ClassSubjects { get; set; } = new();
    }

    public class UpdateClassDTO
    {
        [Required(ErrorMessage = "Class name is required")]
        [StringLength(50, ErrorMessage = "Class name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grade level is required")]
        [StringLength(20, ErrorMessage = "Grade level cannot exceed 20 characters")]
        public string GradeLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Academic year is required")]
        [StringLength(20, ErrorMessage = "Academic year cannot exceed 20 characters")]
        public string AcademicYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section is required")]
        [StringLength(10, ErrorMessage = "Section cannot exceed 10 characters")]
        public string Section { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room number is required")]
        [StringLength(10, ErrorMessage = "Room number cannot exceed 10 characters")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maximum capacity is required")]
        [Range(1, 100, ErrorMessage = "Maximum capacity must be between 1 and 100")]
        public int MaxCapacity { get; set; }

        public Guid? TeacherId { get; set; }

        public decimal MonthlyFee { get; set; }
        public decimal YearlyFee { get; set; }
        public string FeeType { get; set; } = "Monthly";

        [Required(ErrorMessage = "Minimum subjects is required")]
        [Range(1, 20, ErrorMessage = "Minimum subjects must be between 1 and 20")]
        public int MinimumSubjects { get; set; } = 5;

        /// <summary>Optional. When provided, used for ClassSubjects to satisfy FK.</summary>
        public Guid? AcademicYearId { get; set; }

        public List<Guid> SubjectIds { get; set; } = new();
        public List<Guid> CompulsorySubjectIds { get; set; } = new();
        public List<ClassSubjectDTO> ClassSubjects { get; set; } = new();
    }

    public class ClassSubjectDTO
    {
        public Guid SubjectId { get; set; }
        public bool IsCompulsory { get; set; } = false;
    }
}
