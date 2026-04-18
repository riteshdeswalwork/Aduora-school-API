using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Academic
{
    public interface IPeriodManager
    {
        Task<IEnumerable<PeriodDTO>> GetAllPeriodsAsync(Guid? academicYearId = null);
        Task<IEnumerable<PeriodDTO>> GetPeriodsByDayAsync(int dayOfWeek, Guid? academicYearId = null);
        Task<IEnumerable<PeriodDTO>> GetPeriodsForClassAsync(Guid classId, int? dayOfWeek = null, Guid? academicYearId = null);
        Task<PeriodDTO?> GetPeriodByIdAsync(Guid id);
        Task<PeriodDTO> CreatePeriodAsync(CreatePeriodDTO createPeriodDto, Guid createdByUserId);
        Task<PeriodDTO?> UpdatePeriodAsync(Guid id, UpdatePeriodDTO updatePeriodDto);
        Task<bool> DeletePeriodAsync(Guid id);
        Task<bool> PeriodExistsAsync(Guid id);
        Task<IEnumerable<PeriodDTO>> GetActivePeriodsAsync(Guid? academicYearId = null);
        Task<IEnumerable<PeriodDTO>> GetPeriodsByTypeAsync(string periodType, Guid? academicYearId = null);
        Task<bool> HasTimeConflictAsync(TimeSpan startTime, TimeSpan endTime, int dayOfWeek, Guid? excludePeriodId = null, Guid? academicYearId = null);
        Task<IEnumerable<PeriodDTO>> GetOrderedPeriodsAsync(int dayOfWeek, Guid? academicYearId = null);
    }
}
