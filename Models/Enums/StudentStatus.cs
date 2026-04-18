using System.ComponentModel;

namespace sms.api.Models.Enums
{
    public enum StudentStatus
    {
        [Description("Active")]
        Active = 1,
        
        [Description("Inactive")]
        Inactive = 2,
        
        [Description("Pass Out")]
        PassOut = 3,
        
        [Description("Left")]
        Left = 4
    }
    
    public static class StudentStatusExtensions
    {
        public static string GetDisplayName(this StudentStatus status)
        {
            return status switch
            {
                StudentStatus.Active => "Active",
                StudentStatus.Inactive => "Inactive", 
                StudentStatus.PassOut => "Pass Out",
                StudentStatus.Left => "Left",
                _ => "Unknown"
            };
        }
        
        public static string GetDescription(this StudentStatus status)
        {
            return status switch
            {
                StudentStatus.Active => "Student is currently enrolled and attending classes",
                StudentStatus.Inactive => "Student is temporarily inactive but can be reactivated",
                StudentStatus.PassOut => "Student has completed their studies and passed out",
                StudentStatus.Left => "Student has left the school before completion",
                _ => "Unknown status"
            };
        }
        
        public static string GetBadgeClass(this StudentStatus status)
        {
            return status switch
            {
                StudentStatus.Active => "bg-success",
                StudentStatus.Inactive => "bg-warning",
                StudentStatus.PassOut => "bg-primary",
                StudentStatus.Left => "bg-secondary",
                _ => "bg-light"
            };
        }
        
        public static bool IsCountedInClass(this StudentStatus status)
        {
            return status == StudentStatus.Active || status == StudentStatus.Inactive;
        }
        
        public static bool CanAttendClasses(this StudentStatus status)
        {
            return status == StudentStatus.Active;
        }
        
        public static bool CanPayFees(this StudentStatus status)
        {
            return status == StudentStatus.Active || status == StudentStatus.Inactive;
        }
    }
}
