using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using sms.api.Models.Enums;

namespace sms.api.Models
{
    public class Student
    {
        [Key]
        public Guid Id { get; set; }

        public string RegistrationNumber { get; set; } = string.Empty;

        public string RollNumber { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        // Personal Details
        public string BloodGroup { get; set; } = string.Empty;

        public string AadhaarNumber { get; set; } = string.Empty;

        public string Religion { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty; // SC/ST/OBC/General

        // Parent/Guardian Details
        public string FatherName { get; set; } = string.Empty;

        public string MotherName { get; set; } = string.Empty;

        public string GuardianName { get; set; } = string.Empty;

        public string GuardianContact { get; set; } = string.Empty;

        public string ParentName { get; set; } = string.Empty;

        public string ParentPhone { get; set; } = string.Empty;

        public decimal AnnualIncome { get; set; }

        // Address Details
        public string Address { get; set; } = string.Empty; // Permanent Address

        public string CurrentAddress { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        // School Details
        public string AdmissionNumber { get; set; } = string.Empty;

        public string PreviousSchoolName { get; set; } = string.Empty;

        public Guid? ClassId { get; set; }

        // Bus Transportation
        public Guid? BusStopId { get; set; }

        public decimal MonthlyFee { get; set; }

        public string CurrentGrade { get; set; } = string.Empty;

        public DateTime EnrollmentDate { get; set; }

        public DateTime AdmissionDate { get; set; }

        public StudentStatus Status { get; set; } = StudentStatus.Active;

        public DateTime? StatusChangedAt { get; set; }

        public string? StatusChangeReason { get; set; }

        public Guid? StatusChangedBy { get; set; }

        // Backward compatibility property
        public bool IsActive => Status == StudentStatus.Active;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to User
        public Guid UserId { get; set; }

        // Navigation property to User
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        /// <summary>Not mapped. Used when creating to assign role (UserRole).</summary>
        [NotMapped]
        public Guid? RoleId { get; set; }

        // Navigation property to Class
        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }

        // Navigation property to BusStop
        [ForeignKey("BusStopId")]
        public virtual BusStop? BusStop { get; set; }

        // Navigation property for status changed by user
        [ForeignKey("StatusChangedBy")]
        public virtual User? StatusChangedByUser { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        // Navigation property for ClassEnrollments
        [JsonIgnore]
        public virtual ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
    }
}
