using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Academic
{
    public interface IAcademicYearManager : IGenericManager<AcademicYear>
    {
        Task<IEnumerable<AcademicYearDTO>> GetAllAcademicYearsAsync();
        Task<AcademicYearDTO?> GetAcademicYearByIdAsync(Guid id);
        Task<AcademicYearDTO?> GetCurrentAcademicYearAsync();
        Task<AcademicYearDTO?> GetAcademicYearByCodeAsync(string yearCode);
        Task<AcademicYearDTO> CreateAcademicYearAsync(CreateAcademicYearDTO createDto, Guid createdByUserId);
        Task<AcademicYearDTO?> UpdateAcademicYearAsync(Guid id, UpdateAcademicYearDTO updateDto);
        Task<bool> DeleteAcademicYearAsync(Guid id);
        Task<bool> SetCurrentAcademicYearAsync(Guid academicYearId);
        Task<bool> ActivateAcademicYearAsync(Guid academicYearId);
        Task<bool> DeactivateAcademicYearAsync(Guid academicYearId);
        Task<bool> StartNewAcademicYearAsync(Guid newAcademicYearId, Guid userId);
        Task<bool> CloseAcademicYearAsync(Guid academicYearId, Guid userId);
        Task<AcademicYearSummaryDTO> GetAcademicYearSummaryAsync(Guid academicYearId);
        Task<IEnumerable<AcademicYearDTO>> GetAcademicYearsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> IsAcademicYearCodeUniqueAsync(string yearCode, Guid? excludeId = null);
        Task<bool> HasOverlappingDatesAsync(DateTime startDate, DateTime endDate, Guid? excludeId = null);
        Task<bool> CanDeleteAcademicYearAsync(Guid academicYearId);
    }
}
