using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Calendar
{
    public interface ICalendarManager
    {
        Task<CalendarViewDTO> GetCalendarViewAsync(Guid userId, CalendarFilterDTO filter);
        Task<IEnumerable<CalendarItemDTO>> GetCalendarItemsAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<CalendarItemDTO>> GetPersonalCalendarItemsAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<CalendarItemDTO>> GetSchoolCalendarItemsAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetCalendarSummaryAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<bool> HasConflictAsync(DateTime startDate, DateTime endDate, Guid? excludeId = null, string? itemType = null);
    }
}
