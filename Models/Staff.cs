using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sms.api.Models
{
    public class Staff
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign key to Designation
        public Guid? DesignationId { get; set; }

        public string EmployeeId { get; set; } = string.Empty;

        public DateTime DateOfJoining { get; set; }

        public string Address { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Qualification { get; set; } = string.Empty;

        public decimal Salary { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Foreign key to User
        public Guid UserId { get; set; }

        // Navigation property to User
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Navigation property to Designation
        [ForeignKey("DesignationId")]
        public virtual Designation? Designation { get; set; }

        /// <summary>Not mapped. Used when creating/updating to assign role (UserRole).</summary>
        [NotMapped]
        public Guid? RoleId { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

        [JsonIgnore]
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
