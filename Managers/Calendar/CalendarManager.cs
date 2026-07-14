using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Helpers;
using sms.api.Interfaces.Shared;
using sms.api.Interfaces.Staff;
using sms.api.Interfaces.Student;
using sms.api.Managers.Shared;
using sms.api.Models.DTOs;

namespace sms.api.Managers.Calendar
{
    public class CalendarManager : ICalendarManager
    {
        private readonly ApplicationDbContext _context;
        private readonly IHolidayManager _holidayManager;
        private readonly IEventManager _eventManager;
        private readonly IWeekOffManager _weekOffManager;
        private readonly IStudentLeaveManager _studentLeaveManager;
        private readonly IStaffLeaveManager _staffLeaveManager;

        public CalendarManager(
            ApplicationDbContext context,
            IHolidayManager holidayManager,
            IEventManager eventManager,
            IWeekOffManager weekOffManager,
            IStudentLeaveManager studentLeaveManager,
            IStaffLeaveManager staffLeaveManager)
        {
            _context = context;
            _holidayManager = holidayManager;
            _eventManager = eventManager;
            _weekOffManager = weekOffManager;
            _studentLeaveManager = studentLeaveManager;
            _staffLeaveManager = staffLeaveManager;
        }

        public async Task<CalendarViewDTO> GetCalendarViewAsync(Guid userId, CalendarFilterDTO filter)
        {
            var startDate = filter.StartDate ?? DateTime.Today.AddDays(-30);
            var endDate = filter.EndDate ?? DateTime.Today.AddDays(30);

            var items = await GetCalendarItemsAsync(userId, startDate, endDate);

            // Apply filters
            if (filter.Types != null && filter.Types.Any())
            {
                items = items.Where(i => filter.Types.Contains(i.Type));
            }

            if (filter.SubTypes != null && filter.SubTypes.Any())
            {
                items = items.Where(i => filter.SubTypes.Contains(i.SubType));
            }

            if (filter.ClassIds != null && filter.ClassIds.Any())
            {
                items = items.Where(i => i.IsSchoolWide ||
                    (i.ClassIds != null && i.ClassIds.Any(cId => filter.ClassIds.Contains(cId))));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                items = items.Where(i => i.Status == filter.Status);
            }

            if (filter.IncludeSchoolWideOnly)
            {
                items = items.Where(i => i.IsSchoolWide);
            }

            var itemsList = items.OrderBy(i => i.StartDate).ToList();

            var itemCounts = itemsList
                .GroupBy(i => i.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            return new CalendarViewDTO
            {
                Items = itemsList,
                StartDate = startDate,
                EndDate = endDate,
                ViewType = "Month",
                TotalItems = itemsList.Count,
                ItemCounts = itemCounts
            };
        }

        public async Task<IEnumerable<CalendarItemDTO>> GetCalendarItemsAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var items = new List<CalendarItemDTO>();

            // Get holidays
            var holidays = await _holidayManager.GetHolidaysForUserAsync(userId, startDate, endDate);
            items.AddRange(holidays.Select(h => new CalendarItemDTO
            {
                Id = h.Id,
                Title = h.Name,
                Description = h.Description,
                StartDate = h.StartDate,
                EndDate = h.EndDate,
                Type = "Holiday",
                SubType = h.HolidayType,
                Priority = "Normal",
                Status = h.IsActive ? "Active" : "Inactive",
                IsSchoolWide = h.IsSchoolWide,
                ClassIds = h.ClassIds,
                CreatedByName = h.CreatedByName,
                CreatedAt = h.CreatedAt
            }));

            // Get events
            var events = await _eventManager.GetEventsForUserAsync(userId, startDate, endDate);
            items.AddRange(events.Select(e => new CalendarItemDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Type = "Event",
                SubType = e.EventType,
                Priority = e.Priority,
                Status = e.IsActive ? "Active" : "Inactive",
                IsSchoolWide = e.IsSchoolWide,
                ClassIds = e.ClassIds,
                CreatedByName = e.CreatedByName,
                Location = e.Location,
                AttachmentUrl = e.AttachmentUrl,
                RequiresAttendance = e.RequiresAttendance,
                CreatedAt = e.CreatedAt
            }));

            // Get personal leaves
            var personalItems = await GetPersonalCalendarItemsAsync(userId, startDate, endDate);
            items.AddRange(personalItems);

            // Get weekoffs as calendar items
            var weekOffs = await _weekOffManager.GetWeekOffsForUserAsync(userId);
            var weekOffItems = GenerateWeekOffItems(weekOffs, startDate, endDate);
            items.AddRange(weekOffItems);

            return items.OrderBy(i => i.StartDate);
        }

        public async Task<IEnumerable<CalendarItemDTO>> GetPersonalCalendarItemsAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var items = new List<CalendarItemDTO>();

            var user = await _context.Users
                .Include(u => u.Student)
                .Include(u => u.Staff)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Student != null)
            {
                var studentLeaves = await _studentLeaveManager.GetLeavesByStudentIdAsync(user.Student.Id);
                var filteredLeaves = studentLeaves.Where(l =>
                    l.StartDate <= endDate && l.EndDate >= startDate);

                items.AddRange(filteredLeaves.Select(l => new CalendarItemDTO
                {
                    Id = l.Id,
                    Title = $"My Leave - {l.Reason}",
                    Description = l.Reason,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Type = "Leave",
                    SubType = "Student",
                    Priority = "Normal",
                    Status = l.Status,
                    IsSchoolWide = false,
                    CreatedByName = l.Student?.User?.FullName ?? "Unknown",
                    CreatedAt = l.CreatedAt
                }));
            }
            else if (user?.Staff != null)
            {
                var staffLeaves = await _staffLeaveManager.GetLeavesByStaffIdAsync(user.Staff.Id);
                var filteredLeaves = staffLeaves.Where(l =>
                    l.StartDate <= endDate && l.EndDate >= startDate);

                items.AddRange(filteredLeaves.Select(l => new CalendarItemDTO
                {
                    Id = l.Id,
                    Title = $"My Leave - {l.Reason}",
                    Description = l.Reason,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Type = "Leave",
                    SubType = l.LeaveType,
                    Priority = "Normal",
                    Status = l.Status,
                    IsSchoolWide = false,
                    CreatedByName = l.Staff?.User?.FullName ?? "Unknown",
                    CreatedAt = l.CreatedAt
                }));
            }

            return items;
        }

        public async Task<IEnumerable<CalendarItemDTO>> GetSchoolCalendarItemsAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var items = new List<CalendarItemDTO>();

            // Get school-wide holidays
            var holidays = await _holidayManager.GetHolidaysByDateRangeAsync(startDate, endDate);
            items.AddRange(holidays.Where(h => h.IsSchoolWide).Select(h => new CalendarItemDTO
            {
                Id = h.Id,
                Title = h.Name,
                Description = h.Description,
                StartDate = h.StartDate,
                EndDate = h.EndDate,
                Type = "Holiday",
                SubType = h.HolidayType,
                Priority = "Normal",
                Status = h.IsActive ? "Active" : "Inactive",
                IsSchoolWide = h.IsSchoolWide,
                CreatedByName = h.CreatedByName,
                CreatedAt = h.CreatedAt
            }));

            // Get school-wide events
            var events = await _eventManager.GetEventsByDateRangeAsync(startDate, endDate);
            items.AddRange(events.Where(e => e.IsSchoolWide).Select(e => new CalendarItemDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Type = "Event",
                SubType = e.EventType,
                Priority = e.Priority,
                Status = e.IsActive ? "Active" : "Inactive",
                IsSchoolWide = e.IsSchoolWide,
                CreatedByName = e.CreatedByName,
                Location = e.Location,
                AttachmentUrl = e.AttachmentUrl,
                RequiresAttendance = e.RequiresAttendance,
                CreatedAt = e.CreatedAt
            }));

            return items.OrderBy(i => i.StartDate);
        }

        public async Task<Dictionary<string, int>> GetCalendarSummaryAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var items = await GetCalendarItemsAsync(userId, startDate, endDate);

            return items
                .GroupBy(i => i.Type)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<bool> HasConflictAsync(DateTime startDate, DateTime endDate, Guid? excludeId = null, string? itemType = null)
        {
            // Check for holiday conflicts
            var holidays = await _context.Holidays
                .Where(h => h.IsActive &&
                           ((h.StartDate >= startDate && h.StartDate <= endDate) ||
                            (h.EndDate >= startDate && h.EndDate <= endDate) ||
                            (h.StartDate <= startDate && h.EndDate >= endDate)))
                .ToListAsync();

            if (excludeId.HasValue && itemType == "Holiday")
            {
                holidays = holidays.Where(h => h.Id != excludeId.Value).ToList();
            }

            if (holidays.Any()) return true;

            // Check for event conflicts (only high priority events)
            var events = await _context.Events
                .Where(e => e.IsActive && e.Priority == "High" &&
                           ((e.StartDate >= startDate && e.StartDate <= endDate) ||
                            (e.EndDate >= startDate && e.EndDate <= endDate) ||
                            (e.StartDate <= startDate && e.EndDate >= endDate)))
                .ToListAsync();

            if (excludeId.HasValue && itemType == "Event")
            {
                events = events.Where(e => e.Id != excludeId.Value).ToList();
            }

            return events.Any();
        }

        private static IEnumerable<CalendarItemDTO> GenerateWeekOffItems(IEnumerable<WeekOffDTO> weekOffs, DateTime startDate, DateTime endDate)
        {
            var items = new List<CalendarItemDTO>();

            foreach (var weekOff in weekOffs)
            {
                var current = startDate.Date;
                while (current <= endDate.Date)
                {
                    if (WeekOffPatternHelper.MatchesDate(current, weekOff))
                    {
                        items.Add(new CalendarItemDTO
                        {
                            Id = weekOff.Id,
                            SourceId = weekOff.Id,
                            Title = weekOff.Name,
                            Description = string.IsNullOrWhiteSpace(weekOff.Description)
                                ? WeekOffPatternHelper.PatternLabel(weekOff)
                                : weekOff.Description,
                            StartDate = current,
                            EndDate = current,
                            Type = "WeekOff",
                            SubType = weekOff.DayName,
                            Priority = "Normal",
                            Status = weekOff.IsActive ? "Active" : "Inactive",
                            IsSchoolWide = weekOff.IsSchoolWide,
                            ClassIds = weekOff.ClassIds,
                            CreatedByName = weekOff.CreatedByName,
                            CreatedAt = weekOff.CreatedAt
                        });
                    }
                    current = current.AddDays(1);
                }
            }

            return items;
        }
    }
}
