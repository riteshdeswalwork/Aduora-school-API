namespace sms.api.Models.DTOs
{
    public class CalendarItemDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; } = string.Empty; // Holiday, Event, Leave, WeekOff
        public string SubType { get; set; } = string.Empty; // HolidayType, EventType, LeaveType, etc.
        public string Priority { get; set; } = "Normal";
        public string Status { get; set; } = string.Empty; // For leaves: Pending, Approved, Rejected
        public bool IsSchoolWide { get; set; } = true;
        public List<Guid>? ClassIds { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool RequiresAttendance { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }

    public class CalendarViewDTO
    {
        public List<CalendarItemDTO> Items { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ViewType { get; set; } = "Month"; // Month, Week, Day
        public int TotalItems { get; set; }
        public Dictionary<string, int> ItemCounts { get; set; } = new();
    }

    public class CalendarFilterDTO
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? Types { get; set; } // Holiday, Event, Leave, WeekOff
        public List<string>? SubTypes { get; set; }
        public List<Guid>? ClassIds { get; set; }
        public bool IncludePersonalLeaves { get; set; } = true;
        public bool IncludeSchoolWideOnly { get; set; } = false;
        public string? Status { get; set; } // For filtering leaves by status
    }
}
