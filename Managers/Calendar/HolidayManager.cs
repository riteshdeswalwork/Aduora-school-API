using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using sms.api.Data;
using sms.api.Interfaces.Calendar;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Managers.Calendar
{
    public class HolidayManager : IHolidayManager
    {
        private readonly ApplicationDbContext _context;

        public HolidayManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HolidayDTO>> GetAllHolidaysAsync()
        {
            var holidays = await _context.Holidays
                .Include(h => h.CreatedBy)
                .OrderBy(h => h.StartDate)
                .ToListAsync();

            return holidays.Select(MapToDTO);
        }

        public async Task<IEnumerable<HolidayDTO>> GetHolidaysByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var holidays = await _context.Holidays
                .Include(h => h.CreatedBy)
                .Where(h => h.IsActive &&
                           ((h.StartDate >= startDate && h.StartDate <= endDate) ||
                            (h.EndDate >= startDate && h.EndDate <= endDate) ||
                            (h.StartDate <= startDate && h.EndDate >= endDate)))
                .OrderBy(h => h.StartDate)
                .ToListAsync();

            return holidays.Select(MapToDTO);
        }

        public async Task<IEnumerable<HolidayDTO>> GetHolidaysForUserAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var user = await _context.Users
                .Include(u => u.Student)
                .Include(u => u.Staff)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return new List<HolidayDTO>();

            var query = _context.Holidays
                .Include(h => h.CreatedBy)
                .Where(h => h.IsActive);

            // Filter by date range if provided
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(h =>
                    ((h.StartDate >= startDate && h.StartDate <= endDate) ||
                     (h.EndDate >= startDate && h.EndDate <= endDate) ||
                     (h.StartDate <= startDate && h.EndDate >= endDate)));
            }

            var holidays = await query.OrderBy(h => h.StartDate).ToListAsync();

            // Filter holidays based on user's classes
            var filteredHolidays = new List<Holiday>();
           
            foreach (var holiday in holidays)
            {
                if (holiday.IsSchoolWide)
                {
                    filteredHolidays.Add(holiday);
                }
                else if (!string.IsNullOrEmpty(holiday.ClassIds))
                {
                    var classIds = JsonSerializer.Deserialize<List<Guid>>(holiday.ClassIds) ?? new List<Guid>();
                   
                    if (user.Student != null)
                    {
                        var studentClassIds = await _context.ClassEnrollments
                            .Where(ce => ce.StudentId == user.Student.Id)
                            .Select(ce => ce.ClassId)
                            .ToListAsync();
                       
                        if (classIds.Any(cId => studentClassIds.Contains(cId)))
                        {
                            filteredHolidays.Add(holiday);
                        }
                    }
                    else if (user.Staff != null)
                    {
                        var staffClassIds = await _context.Classes
                            .Where(c => c.TeacherId == user.Staff.Id)
                            .Select(c => c.Id)
                            .ToListAsync();
                       
                        if (classIds.Any(cId => staffClassIds.Contains(cId)))
                        {
                            filteredHolidays.Add(holiday);
                        }
                    }
                }
            }

            return filteredHolidays.Select(MapToDTO);
        }

        public async Task<HolidayDTO?> GetHolidayByIdAsync(Guid id)
        {
            var holiday = await _context.Holidays
                .Include(h => h.CreatedBy)
                .FirstOrDefaultAsync(h => h.Id == id);

            return holiday != null ? MapToDTO(holiday) : null;
        }

        public async Task<HolidayDTO> CreateHolidayAsync(CreateHolidayDTO createHolidayDto, Guid createdByUserId)
        {
            var holiday = new Holiday
            {
                Id = Guid.NewGuid(),
                Name = createHolidayDto.Name,
                Description = createHolidayDto.Description,
                StartDate = createHolidayDto.StartDate,
                EndDate = createHolidayDto.EndDate,
                HolidayType = createHolidayDto.HolidayType,
                IsRecurring = createHolidayDto.IsRecurring,
                RecurrencePattern = createHolidayDto.RecurrencePattern,
                IsSchoolWide = createHolidayDto.IsSchoolWide,
                ClassIds = createHolidayDto.ClassIds != null && createHolidayDto.ClassIds.Any()
                    ? JsonSerializer.Serialize(createHolidayDto.ClassIds)
                    : null,
                IsActive = createHolidayDto.IsActive,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();

            return await GetHolidayByIdAsync(holiday.Id) ?? throw new InvalidOperationException("Failed to create holiday");
        }

        public async Task<HolidayDTO?> UpdateHolidayAsync(Guid id, UpdateHolidayDTO updateHolidayDto)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday == null) return null;

            if (!string.IsNullOrEmpty(updateHolidayDto.Name))
                holiday.Name = updateHolidayDto.Name;
           
            if (!string.IsNullOrEmpty(updateHolidayDto.Description))
                holiday.Description = updateHolidayDto.Description;
           
            if (updateHolidayDto.StartDate.HasValue)
                holiday.StartDate = updateHolidayDto.StartDate.Value;
           
            if (updateHolidayDto.EndDate.HasValue)
                holiday.EndDate = updateHolidayDto.EndDate.Value;
           
            if (!string.IsNullOrEmpty(updateHolidayDto.HolidayType))
                holiday.HolidayType = updateHolidayDto.HolidayType;
           
            if (updateHolidayDto.IsRecurring.HasValue)
                holiday.IsRecurring = updateHolidayDto.IsRecurring.Value;
           
            if (!string.IsNullOrEmpty(updateHolidayDto.RecurrencePattern))
                holiday.RecurrencePattern = updateHolidayDto.RecurrencePattern;
           
            if (updateHolidayDto.IsSchoolWide.HasValue)
                holiday.IsSchoolWide = updateHolidayDto.IsSchoolWide.Value;
           
            if (updateHolidayDto.ClassIds != null)
                holiday.ClassIds = updateHolidayDto.ClassIds.Any()
                    ? JsonSerializer.Serialize(updateHolidayDto.ClassIds)
                    : null;
           
            if (updateHolidayDto.IsActive.HasValue)
                holiday.IsActive = updateHolidayDto.IsActive.Value;

            holiday.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetHolidayByIdAsync(id);
        }

        public async Task<bool> DeleteHolidayAsync(Guid id)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday == null) return false;

            _context.Holidays.Remove(holiday);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HolidayExistsAsync(Guid id)
        {
            return await _context.Holidays.AnyAsync(h => h.Id == id);
        }

        public async Task<IEnumerable<HolidayDTO>> GetActiveHolidaysAsync()
        {
            var holidays = await _context.Holidays
                .Include(h => h.CreatedBy)
                .Where(h => h.IsActive)
                .OrderBy(h => h.StartDate)
                .ToListAsync();

            return holidays.Select(MapToDTO);
        }

        public async Task<IEnumerable<HolidayDTO>> GetHolidaysByTypeAsync(string holidayType)
        {
            var holidays = await _context.Holidays
                .Include(h => h.CreatedBy)
                .Where(h => h.IsActive && h.HolidayType == holidayType)
                .OrderBy(h => h.StartDate)
                .ToListAsync();

            return holidays.Select(MapToDTO);
        }

        public async Task<IEnumerable<HolidayDTO>> GetRecurringHolidaysAsync()
        {
            var holidays = await _context.Holidays
                .Include(h => h.CreatedBy)
                .Where(h => h.IsActive && h.IsRecurring)
                .OrderBy(h => h.StartDate)
                .ToListAsync();

            return holidays.Select(MapToDTO);
        }

        private static HolidayDTO MapToDTO(Holiday holiday)
        {
            return new HolidayDTO
            {
                Id = holiday.Id,
                Name = holiday.Name,
                Description = holiday.Description,
                StartDate = holiday.StartDate,
                EndDate = holiday.EndDate,
                HolidayType = holiday.HolidayType,
                IsRecurring = holiday.IsRecurring,
                RecurrencePattern = holiday.RecurrencePattern,
                IsSchoolWide = holiday.IsSchoolWide,
                ClassIds = !string.IsNullOrEmpty(holiday.ClassIds)
                    ? JsonSerializer.Deserialize<List<Guid>>(holiday.ClassIds)
                    : null,
                IsActive = holiday.IsActive,
                CreatedByName = holiday.CreatedBy?.FullName ?? "Unknown",
                CreatedAt = holiday.CreatedAt,
                UpdatedAt = holiday.UpdatedAt
            };
        }
    }
}
