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
    public class WeekOffManager : IWeekOffManager
    {
        private readonly ApplicationDbContext _context;

        public WeekOffManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WeekOffDTO>> GetAllWeekOffsAsync()
        {
            var weekOffs = await _context.WeekOffs
                .Include(w => w.CreatedBy)
                .OrderBy(w => w.DayOfWeek)
                .ToListAsync();

            return weekOffs.Select(MapToDTO);
        }

        public async Task<IEnumerable<WeekOffDTO>> GetWeekOffsForUserAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Student)
                .Include(u => u.Staff)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return new List<WeekOffDTO>();

            var weekOffs = await _context.WeekOffs
                .Include(w => w.CreatedBy)
                .Where(w => w.IsActive &&
                           (w.EffectiveTo == null || w.EffectiveTo >= DateTime.UtcNow))
                .OrderBy(w => w.DayOfWeek)
                .ToListAsync();

            // Filter weekoffs based on user's classes
            var filteredWeekOffs = new List<WeekOff>();
           
            foreach (var weekOff in weekOffs)
            {
                if (weekOff.IsSchoolWide)
                {
                    filteredWeekOffs.Add(weekOff);
                }
                else if (!string.IsNullOrEmpty(weekOff.ClassIds))
                {
                    var classIds = JsonSerializer.Deserialize<List<Guid>>(weekOff.ClassIds) ?? new List<Guid>();
                   
                    if (user.Student != null)
                    {
                        var studentClassIds = await _context.ClassEnrollments
                            .Where(ce => ce.StudentId == user.Student.Id)
                            .Select(ce => ce.ClassId)
                            .ToListAsync();
                       
                        if (classIds.Any(cId => studentClassIds.Contains(cId)))
                        {
                            filteredWeekOffs.Add(weekOff);
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
                            filteredWeekOffs.Add(weekOff);
                        }
                    }
                }
            }

            return filteredWeekOffs.Select(MapToDTO);
        }

        public async Task<WeekOffDTO?> GetWeekOffByIdAsync(Guid id)
        {
            var weekOff = await _context.WeekOffs
                .Include(w => w.CreatedBy)
                .FirstOrDefaultAsync(w => w.Id == id);

            return weekOff != null ? MapToDTO(weekOff) : null;
        }

        public async Task<WeekOffDTO> CreateWeekOffAsync(CreateWeekOffDTO createWeekOffDto, Guid createdByUserId)
        {
            var weekOff = new WeekOff
            {
                Id = Guid.NewGuid(),
                Name = createWeekOffDto.Name,
                Description = createWeekOffDto.Description,
                DayOfWeek = createWeekOffDto.DayOfWeek,
                IsSchoolWide = createWeekOffDto.IsSchoolWide,
                ClassIds = createWeekOffDto.ClassIds != null && createWeekOffDto.ClassIds.Any()
                    ? JsonSerializer.Serialize(createWeekOffDto.ClassIds)
                    : null,
                IsActive = createWeekOffDto.IsActive,
                EffectiveFrom = createWeekOffDto.EffectiveFrom,
                EffectiveTo = createWeekOffDto.EffectiveTo,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.WeekOffs.Add(weekOff);
            await _context.SaveChangesAsync();

            return await GetWeekOffByIdAsync(weekOff.Id) ?? throw new InvalidOperationException("Failed to create week off");
        }

        public async Task<WeekOffDTO?> UpdateWeekOffAsync(Guid id, UpdateWeekOffDTO updateWeekOffDto)
        {
            var weekOff = await _context.WeekOffs.FindAsync(id);
            if (weekOff == null) return null;

            if (!string.IsNullOrEmpty(updateWeekOffDto.Name))
                weekOff.Name = updateWeekOffDto.Name;
           
            if (!string.IsNullOrEmpty(updateWeekOffDto.Description))
                weekOff.Description = updateWeekOffDto.Description;
           
            if (updateWeekOffDto.DayOfWeek.HasValue)
                weekOff.DayOfWeek = updateWeekOffDto.DayOfWeek.Value;
           
            if (updateWeekOffDto.IsSchoolWide.HasValue)
                weekOff.IsSchoolWide = updateWeekOffDto.IsSchoolWide.Value;
           
            if (updateWeekOffDto.ClassIds != null)
                weekOff.ClassIds = updateWeekOffDto.ClassIds.Any()
                    ? JsonSerializer.Serialize(updateWeekOffDto.ClassIds)
                    : null;
           
            if (updateWeekOffDto.IsActive.HasValue)
                weekOff.IsActive = updateWeekOffDto.IsActive.Value;
           
            if (updateWeekOffDto.EffectiveFrom.HasValue)
                weekOff.EffectiveFrom = updateWeekOffDto.EffectiveFrom.Value;
           
            if (updateWeekOffDto.EffectiveTo.HasValue)
                weekOff.EffectiveTo = updateWeekOffDto.EffectiveTo.Value;

            weekOff.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetWeekOffByIdAsync(id);
        }

        public async Task<bool> DeleteWeekOffAsync(Guid id)
        {
            var weekOff = await _context.WeekOffs.FindAsync(id);
            if (weekOff == null) return false;

            _context.WeekOffs.Remove(weekOff);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> WeekOffExistsAsync(Guid id)
        {
            return await _context.WeekOffs.AnyAsync(w => w.Id == id);
        }

        public async Task<IEnumerable<WeekOffDTO>> GetActiveWeekOffsAsync()
        {
            var weekOffs = await _context.WeekOffs
                .Include(w => w.CreatedBy)
                .Where(w => w.IsActive &&
                           (w.EffectiveTo == null || w.EffectiveTo >= DateTime.UtcNow))
                .OrderBy(w => w.DayOfWeek)
                .ToListAsync();

            return weekOffs.Select(MapToDTO);
        }

        public async Task<IEnumerable<WeekOffDTO>> GetWeekOffsByDayAsync(int dayOfWeek)
        {
            var weekOffs = await _context.WeekOffs
                .Include(w => w.CreatedBy)
                .Where(w => w.IsActive && w.DayOfWeek == dayOfWeek &&
                           (w.EffectiveTo == null || w.EffectiveTo >= DateTime.UtcNow))
                .OrderBy(w => w.Name)
                .ToListAsync();

            return weekOffs.Select(MapToDTO);
        }

        public async Task<bool> IsWeekOffAsync(DateTime date, Guid? userId = null)
        {
            var dayOfWeek = (int)date.DayOfWeek;
           
            var query = _context.WeekOffs
                .Where(w => w.IsActive &&
                           w.DayOfWeek == dayOfWeek &&
                           w.EffectiveFrom <= date &&
                           (w.EffectiveTo == null || w.EffectiveTo >= date));

            if (userId.HasValue)
            {
                var user = await _context.Users
                    .Include(u => u.Student)
                    .Include(u => u.Staff)
                    .FirstOrDefaultAsync(u => u.Id == userId.Value);

                if (user != null)
                {
                    var weekOffs = await query.ToListAsync();
                   
                    foreach (var weekOff in weekOffs)
                    {
                        if (weekOff.IsSchoolWide) return true;
                       
                        if (!string.IsNullOrEmpty(weekOff.ClassIds))
                        {
                            var classIds = JsonSerializer.Deserialize<List<Guid>>(weekOff.ClassIds) ?? new List<Guid>();
                           
                            if (user.Student != null)
                            {
                                var studentClassIds = await _context.ClassEnrollments
                                    .Where(ce => ce.StudentId == user.Student.Id)
                                    .Select(ce => ce.ClassId)
                                    .ToListAsync();
                               
                                if (classIds.Any(cId => studentClassIds.Contains(cId)))
                                    return true;
                            }
                            else if (user.Staff != null)
                            {
                                var staffClassIds = await _context.Classes
                                    .Where(c => c.TeacherId == user.Staff.Id)
                                    .Select(c => c.Id)
                                    .ToListAsync();
                               
                                if (classIds.Any(cId => staffClassIds.Contains(cId)))
                                    return true;
                            }
                        }
                    }
                }
            }
            else
            {
                return await query.Where(w => w.IsSchoolWide).AnyAsync();
            }

            return false;
        }

        private static WeekOffDTO MapToDTO(WeekOff weekOff)
        {
            var dayNames = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
           
            return new WeekOffDTO
            {
                Id = weekOff.Id,
                AcademicYearId = weekOff.AcademicYearId,
                Name = weekOff.Name,
                Description = weekOff.Description,
                DayOfWeek = weekOff.DayOfWeek,
                DayName = dayNames[weekOff.DayOfWeek],
                IsSchoolWide = weekOff.IsSchoolWide,
                ClassIds = !string.IsNullOrEmpty(weekOff.ClassIds)
                    ? JsonSerializer.Deserialize<List<Guid>>(weekOff.ClassIds)
                    : null,
                IsActive = weekOff.IsActive,
                EffectiveFrom = weekOff.EffectiveFrom,
                EffectiveTo = weekOff.EffectiveTo,
                CreatedByName = weekOff.CreatedBy?.FullName ?? "Unknown",
                CreatedAt = weekOff.CreatedAt,
                UpdatedAt = weekOff.UpdatedAt
            };
        }
    }
}
