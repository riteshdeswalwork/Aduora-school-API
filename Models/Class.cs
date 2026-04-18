using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sms.api.Models
{
    public class Class
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string GradeLevel { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Section { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 100)]
        public int MaxCapacity { get; set; }

        public int Capacity { get; set; }

        public int CurrentEnrollment { get; set; }

        public bool IsActive { get; set; }

        public Guid? TeacherId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MonthlyFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal YearlyFee { get; set; }

        [StringLength(20)]
        public string FeeType { get; set; } = string.Empty; // Monthly or Yearly

        [Range(0, 20)]
        public int MinimumSubjects { get; set; } // Minimum subjects required for this class

        [ForeignKey("TeacherId")]
        public Staff? Teacher { get; set; }

        // Navigation properties
        public virtual ICollection<ClassEnrollment> Enrollments { get; set; } = new List<ClassEnrollment>();
        public virtual ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
