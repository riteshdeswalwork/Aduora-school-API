using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using sms.api.Data;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Managers.Academic
{
    public class PeriodManager : IPeriodManager
    {
        private readonly ApplicationDbContext _context;

        public PeriodManager(ApplicationDbContext context)
        {
            _context = context;
        }

        private static IQueryable<Period> ApplyAcademicYear(IQueryable<Period> query, Guid? academicYearId)
        {
            if (academicYearId == null) return query;
            return query.Where(p => p.AcademicYearId == academicYearId);
        }

        public async Task<IEnumerable<PeriodDTO>> GetAllPeriodsAsync(Guid? academicYearId = null)
        {
            var query = _context.Periods
                .Include(p => p.CreatedBy)
                .AsQueryable();
            query = ApplyAcademicYear(query, academicYearId);
            var periods = await query
                .OrderBy(p => p.DayOfWeek)
                .ThenBy(p => p.OrderIndex)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            return periods.Select(MapToDTO);
        }

        public async Task<IEnumerable<PeriodDTO>> GetPeriodsByDayAsync(int dayOfWeek, Guid? academicYearId = null)
        {
            var query = _context.Periods
                .Include(p => p.CreatedBy)
                .Where(p => p.IsActive && p.DayOfWeek == dayOfWeek);
            query = ApplyAcademicYear(query, academicYearId);
            var periods = await query
                .OrderBy(p => p.OrderIndex)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            return periods.Select(MapToDTO);
        }

        public async Task<IEnumerable<PeriodDTO>> GetPeriodsForClassAsync(Guid classId, int? dayOfWeek = null, Guid? academicYearId = null)
        {
            var query = _context.Periods
                .Include(p => p.CreatedBy)
                .Where(p => p.IsActive);
            query = ApplyAcademicYear(query, academicYearId);
            if (dayOfWeek.HasValue)
            {
                query = query.Where(p => p.DayOfWeek == dayOfWeek.Value);
            }

            var periods = await query
                .OrderBy(p => p.DayOfWeek)
                .ThenBy(p => p.OrderIndex)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            // Filter periods based on class scope
            var filteredPeriods = new List<Period>();
           
            foreach (var period in periods)
            {
                if (period.IsSchoolWide)
                {
                    filteredPeriods.Add(period);
                }
                else if (!string.IsNullOrEmpty(period.ClassIds))
                {
                    var classIds = JsonSerializer.Deserialize<List<Guid>>(period.ClassIds) ?? new List<Guid>();
                    if (classIds.Contains(classId))
                    {
                        filteredPeriods.Add(period);
                    }
                }
            }

            return filteredPeriods.Select(MapToDTO);
        }

        public async Task<PeriodDTO?> GetPeriodByIdAsync(Guid id)
        {
            var period = await _context.Periods
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == id);

            return period != null ? MapToDTO(period) : null;
        }

        public async Task<PeriodDTO> CreatePeriodAsync(CreatePeriodDTO createPeriodDto, Guid createdByUserId)
        {
            var period = new Period
            {
                Id = Guid.NewGuid(),
                Name = createPeriodDto.Name,
                Description = createPeriodDto.Description,
                StartTime = createPeriodDto.StartTime,
                EndTime = createPeriodDto.EndTime,
                DayOfWeek = createPeriodDto.DayOfWeek,
                PeriodType = createPeriodDto.PeriodType,
                OrderIndex = createPeriodDto.OrderIndex,
                AcademicYearId = createPeriodDto.AcademicYearId,
                IsSchoolWide = createPeriodDto.IsSchoolWide,
                ClassIds = createPeriodDto.ClassIds != null && createPeriodDto.ClassIds.Any()
                    ? JsonSerializer.Serialize(createPeriodDto.ClassIds)
                    : null,
                IsActive = createPeriodDto.IsActive,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Periods.Add(period);
            await _context.SaveChangesAsync();

            return await GetPeriodByIdAsync(period.Id) ?? throw new InvalidOperationException("Failed to create period");
        }

        public async Task<PeriodDTO?> UpdatePeriodAsync(Guid id, UpdatePeriodDTO updatePeriodDto)
        {
            var period = await _context.Periods.FindAsync(id);
            if (period == null) return null;

            if (!string.IsNullOrEmpty(updatePeriodDto.Name))
                period.Name = updatePeriodDto.Name;
           
            if (!string.IsNullOrEmpty(updatePeriodDto.Description))
                period.Description = updatePeriodDto.Description;
           
            if (updatePeriodDto.StartTime.HasValue)
                period.StartTime = updatePeriodDto.StartTime.Value;
           
            if (updatePeriodDto.EndTime.HasValue)
                period.EndTime = updatePeriodDto.EndTime.Value;
           
            if (updatePeriodDto.DayOfWeek.HasValue)
                period.DayOfWeek = updatePeriodDto.DayOfWeek.Value;
           
            if (!string.IsNullOrEmpty(updatePeriodDto.PeriodType))
                period.PeriodType = updatePeriodDto.PeriodType;
           
            if (updatePeriodDto.OrderIndex.HasValue)
                period.OrderIndex = updatePeriodDto.OrderIndex.Value;

            if (updatePeriodDto.AcademicYearId.HasValue)
                period.AcademicYearId = updatePeriodDto.AcademicYearId;
           
            if (updatePeriodDto.IsSchoolWide.HasValue)
                period.IsSchoolWide = updatePeriodDto.IsSchoolWide.Value;
           
            if (updatePeriodDto.ClassIds != null)
                period.ClassIds = updatePeriodDto.ClassIds.Any()
                    ? JsonSerializer.Serialize(updatePeriodDto.ClassIds)
                    : null;
           
            if (updatePeriodDto.IsActive.HasValue)
                period.IsActive = updatePeriodDto.IsActive.Value;

            period.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetPeriodByIdAsync(id);
        }

        public async Task<bool> DeletePeriodAsync(Guid id)
        {
            var period = await _context.Periods.FindAsync(id);
            if (period == null) return false;

            _context.Periods.Remove(period);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PeriodExistsAsync(Guid id)
        {
            return await _context.Periods.AnyAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PeriodDTO>> GetActivePeriodsAsync(Guid? academicYearId = null)
        {
            var query = _context.Periods
                .Include(p => p.CreatedBy)
                .Where(p => p.IsActive);
            query = ApplyAcademicYear(query, academicYearId);
            var periods = await query
                .OrderBy(p => p.DayOfWeek)
                .ThenBy(p => p.OrderIndex)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            return periods.Select(MapToDTO);
        }

        public async Task<IEnumerable<PeriodDTO>> GetPeriodsByTypeAsync(string periodType, Guid? academicYearId = null)
        {
            var query = _context.Periods
                .Include(p => p.CreatedBy)
                .Where(p => p.IsActive && p.PeriodType == periodType);
            query = ApplyAcademicYear(query, academicYearId);
            var periods = await query
                .OrderBy(p => p.DayOfWeek)
                .ThenBy(p => p.OrderIndex)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            return periods.Select(MapToDTO);
        }

        public async Task<bool> HasTimeConflictAsync(TimeSpan startTime, TimeSpan endTime, int dayOfWeek, Guid? excludePeriodId = null, Guid? academicYearId = null)
        {
            var query = _context.Periods
                .Where(p => p.IsActive &&
                           p.DayOfWeek == dayOfWeek &&
                           ((p.StartTime >= startTime && p.StartTime < endTime) ||
                            (p.EndTime > startTime && p.EndTime <= endTime) ||
                            (p.StartTime <= startTime && p.EndTime >= endTime)));
            query = ApplyAcademicYear(query, academicYearId);
            if (excludePeriodId.HasValue)
            {
                query = query.Where(p => p.Id != excludePeriodId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<PeriodDTO>> GetOrderedPeriodsAsync(int dayOfWeek, Guid? academicYearId = null)
        {
            var query = _context.Periods
                .Include(p => p.CreatedBy)
                .Where(p => p.IsActive && p.DayOfWeek == dayOfWeek);
            query = ApplyAcademicYear(query, academicYearId);
            var periods = await query
                .OrderBy(p => p.OrderIndex)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            return periods.Select(MapToDTO);
        }

        private static PeriodDTO MapToDTO(Period period)
        {
            var dayNames = new[] { "", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
           
            return new PeriodDTO
            {
                Id = period.Id,
                Name = period.Name,
                Description = period.Description,
                StartTime = period.StartTime,
                EndTime = period.EndTime,
                DayOfWeek = period.DayOfWeek,
                DayName = dayNames[period.DayOfWeek],
                PeriodType = period.PeriodType,
                OrderIndex = period.OrderIndex,
                AcademicYearId = period.AcademicYearId,
                IsSchoolWide = period.IsSchoolWide,
                ClassIds = !string.IsNullOrEmpty(period.ClassIds)
                    ? JsonSerializer.Deserialize<List<Guid>>(period.ClassIds)
                    : null,
                IsActive = period.IsActive,
                CreatedByName = period.CreatedBy?.FullName ?? "Unknown",
                CreatedAt = period.CreatedAt,
                UpdatedAt = period.UpdatedAt,
                DurationMinutes = period.DurationMinutes,
                TimeRange = period.TimeRange
            };
        }
    }
}
