using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Calendar
{
    public interface IWeekOffManager
    {
        Task<IEnumerable<WeekOffDTO>> GetAllWeekOffsAsync();
        Task<IEnumerable<WeekOffDTO>> GetWeekOffsForUserAsync(Guid userId);
        Task<WeekOffDTO?> GetWeekOffByIdAsync(Guid id);
        Task<WeekOffDTO> CreateWeekOffAsync(CreateWeekOffDTO createWeekOffDto, Guid createdByUserId);
        Task<WeekOffDTO?> UpdateWeekOffAsync(Guid id, UpdateWeekOffDTO updateWeekOffDto);
        Task<bool> DeleteWeekOffAsync(Guid id);
        Task<bool> WeekOffExistsAsync(Guid id);
        Task<IEnumerable<WeekOffDTO>> GetActiveWeekOffsAsync();
        Task<IEnumerable<WeekOffDTO>> GetWeekOffsByDayAsync(int dayOfWeek);
        Task<bool> IsWeekOffAsync(DateTime date, Guid? userId = null);
    }
}
