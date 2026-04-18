using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Calendar
{
    public interface IHolidayManager
    {
        Task<IEnumerable<HolidayDTO>> GetAllHolidaysAsync();
        Task<IEnumerable<HolidayDTO>> GetHolidaysByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<HolidayDTO>> GetHolidaysForUserAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<HolidayDTO?> GetHolidayByIdAsync(Guid id);
        Task<HolidayDTO> CreateHolidayAsync(CreateHolidayDTO createHolidayDto, Guid createdByUserId);
        Task<HolidayDTO?> UpdateHolidayAsync(Guid id, UpdateHolidayDTO updateHolidayDto);
        Task<bool> DeleteHolidayAsync(Guid id);
        Task<bool> HolidayExistsAsync(Guid id);
        Task<IEnumerable<HolidayDTO>> GetActiveHolidaysAsync();
        Task<IEnumerable<HolidayDTO>> GetHolidaysByTypeAsync(string holidayType);
        Task<IEnumerable<HolidayDTO>> GetRecurringHolidaysAsync();
    }
}
