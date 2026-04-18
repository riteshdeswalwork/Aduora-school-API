using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Academic
{
    public interface ITimetableManager
    {
        Task<IEnumerable<TimetableEntryDTO>> GetAllTimetableEntriesAsync(DateTime? scheduleDate = null, DateTime? endDate = null);
        Task<IEnumerable<TimetableEntryDTO>> GetTimetableByClassAsync(Guid classId, int? dayOfWeek = null, DateTime? scheduleDate = null, DateTime? endDate = null);
        Task<IEnumerable<TimetableEntryDTO>> GetTimetableByTeacherAsync(Guid teacherId, int? dayOfWeek = null, DateTime? scheduleDate = null, DateTime? endDate = null);
        Task<TimetableEntryDTO?> GetTimetableEntryByIdAsync(Guid id);
        Task<TimetableEntryDTO> CreateTimetableEntryAsync(CreateTimetableEntryDTO createTimetableEntryDto, Guid createdByUserId);
        Task<TimetableEntryDTO?> UpdateTimetableEntryAsync(Guid id, UpdateTimetableEntryDTO updateTimetableEntryDto);
        Task<bool> DeleteTimetableEntryAsync(Guid id);
        Task<bool> TimetableEntryExistsAsync(Guid id);
        Task<TimetableViewDTO> GetClassTimetableAsync(Guid classId);
        Task<WeeklyTimetableDTO> GetWeeklyTimetableAsync();
        Task<WeeklyTimetableDTO> GetWeeklyTimetableForUserAsync(Guid userId);
        Task<bool> HasTeacherConflictAsync(TeacherConflictCheckDTO conflictCheck);
        /// <summary>Same rules as <see cref="HasTeacherConflictAsync"/>; when blocked, returns a short hint (class, subject, time).</summary>
        Task<(bool HasConflict, string? BlockingSummary)> CheckTeacherConflictWithSummaryAsync(TeacherConflictCheckDTO conflictCheck);
        /// <summary>Returns an error message if the first Regular period of the day must use the class assigned teacher.</summary>
        Task<string?> ValidateClassTeacherForFirstPeriodAsync(Guid periodId, Guid classId, Guid? teacherId, Guid academicYearId);
        Task<IEnumerable<TimetableEntryDTO>> GetTeacherConflictsAsync(Guid teacherId, Guid periodId, DateTime? specificDate = null);
        Task<bool> AssignSubstituteTeacherAsync(AssignSubstituteDTO assignSubstituteDto, Guid assignedByUserId);
        Task<bool> RemoveSubstituteTeacherAsync(Guid timetableEntryId, DateTime substituteDate);
        Task<IEnumerable<TimetableEntryDTO>> GetSubstituteAssignmentsAsync(DateTime date);
        Task<IEnumerable<TimetableEntryDTO>> GetTeacherSubstituteHistoryAsync(Guid teacherId);
        Task<bool> CreateClassTimetableAsync(Guid classId, List<CreateTimetableEntryDTO> entries, Guid createdByUserId);
        Task<bool> CopyTimetableAsync(Guid fromClassId, Guid toClassId, Guid createdByUserId);
        Task<int> CopyTimetableDayAsync(CopyTimetableDayDTO dto, Guid createdByUserId);
        /// <summary>Returns null if the target calendar day is allowed for timetable copy; otherwise a user-facing reason.</summary>
        Task<string?> ValidateCopyDayTargetDateAsync(Guid academicYearId, DateTime toDate);
    }
}
