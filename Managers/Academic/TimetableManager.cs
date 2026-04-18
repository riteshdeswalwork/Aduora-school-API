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
    public class TimetableManager : ITimetableManager
    {
        private readonly ApplicationDbContext _context;

        public TimetableManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TimetableEntryDTO>> GetAllTimetableEntriesAsync(DateTime? scheduleDate = null, DateTime? endDate = null)
        {
            var query = _context.TimetableEntries
                .Include(te => te.Period)
                .Include(te => te.Class)
                .Include(te => te.Subject)
                .Include(te => te.Teacher).ThenInclude(t => t.User)
                .Include(te => te.SubstituteTeacher).ThenInclude(st => st.User)
                .Include(te => te.CreatedBy)
                .AsQueryable();

            if (scheduleDate.HasValue && endDate.HasValue)
            {
                var s = scheduleDate.Value.Date;
                var e = endDate.Value.Date;
                query = query.Where(te =>
                    te.ScheduleDate.HasValue &&
                    te.ScheduleDate.Value.Date >= s &&
                    te.ScheduleDate.Value.Date <= e);
            }
            else if (scheduleDate.HasValue)
            {
                var d = scheduleDate.Value.Date;
                query = query.Where(te => te.ScheduleDate.HasValue && te.ScheduleDate.Value.Date == d);
            }

            var entries = await query
                .OrderBy(te => te.Period!.DayOfWeek)
                .ThenBy(te => te.Period!.OrderIndex)
                .ThenBy(te => te.Period!.StartTime)
                .ThenBy(te => te.Class!.Name)
                .ToListAsync();

            return entries.Select(MapToDTO);
        }

        public async Task<IEnumerable<TimetableEntryDTO>> GetTimetableByClassAsync(Guid classId, int? dayOfWeek = null, DateTime? scheduleDate = null, DateTime? endDate = null)
        {
            var query = _context.TimetableEntries
                .Include(te => te.Period)
                .Include(te => te.Class)
                .Include(te => te.Subject)
                .Include(te => te.Teacher).ThenInclude(t => t.User)
                .Include(te => te.SubstituteTeacher).ThenInclude(st => st.User)
                .Include(te => te.CreatedBy)
                .Where(te => te.IsActive && te.ClassId == classId);

            if (dayOfWeek.HasValue)
            {
                query = query.Where(te => te.Period.DayOfWeek == dayOfWeek.Value);
            }

            if (scheduleDate.HasValue && endDate.HasValue)
            {
                var s = scheduleDate.Value.Date;
                var e = endDate.Value.Date;
                query = query.Where(te =>
                    te.ScheduleDate.HasValue &&
                    te.ScheduleDate.Value.Date >= s &&
                    te.ScheduleDate.Value.Date <= e);
            }
            else if (scheduleDate.HasValue)
            {
                var d = scheduleDate.Value.Date;
                query = query.Where(te => te.ScheduleDate.HasValue && te.ScheduleDate.Value.Date == d);
            }

            var entries = await query
                .OrderBy(te => te.Period.DayOfWeek)
                .ThenBy(te => te.Period.OrderIndex)
                .ThenBy(te => te.Period.StartTime)
                .ToListAsync();

            return entries.Select(MapToDTO);
        }

        public async Task<IEnumerable<TimetableEntryDTO>> GetTimetableByTeacherAsync(Guid teacherId, int? dayOfWeek = null, DateTime? scheduleDate = null, DateTime? endDate = null)
        {
            var query = _context.TimetableEntries
                .Include(te => te.Period)
                .Include(te => te.Class)
                .Include(te => te.Subject)
                .Include(te => te.Teacher).ThenInclude(t => t.User)
                .Include(te => te.SubstituteTeacher).ThenInclude(st => st.User)
                .Include(te => te.CreatedBy)
                .Where(te => te.IsActive &&
                            (te.TeacherId == teacherId ||
                             (te.SubstituteTeacherId == teacherId &&
                              te.SubstituteDate.HasValue &&
                              te.SubstituteDate.Value.Date == DateTime.Today)));

            if (dayOfWeek.HasValue)
            {
                query = query.Where(te => te.Period.DayOfWeek == dayOfWeek.Value);
            }

            if (scheduleDate.HasValue && endDate.HasValue)
            {
                var s = scheduleDate.Value.Date;
                var e = endDate.Value.Date;
                query = query.Where(te =>
                    te.ScheduleDate.HasValue &&
                    te.ScheduleDate.Value.Date >= s &&
                    te.ScheduleDate.Value.Date <= e);
            }
            else if (scheduleDate.HasValue)
            {
                var d = scheduleDate.Value.Date;
                query = query.Where(te => te.ScheduleDate.HasValue && te.ScheduleDate.Value.Date == d);
            }

            var entries = await query
                .OrderBy(te => te.Period.DayOfWeek)
                .ThenBy(te => te.Period.OrderIndex)
                .ThenBy(te => te.Period.StartTime)
                .ToListAsync();

            return entries.Select(MapToDTO);
        }

        public async Task<TimetableEntryDTO?> GetTimetableEntryByIdAsync(Guid id)
        {
            var entry = await _context.TimetableEntries
                .Include(te => te.Period)
                .Include(te => te.Class)
                .Include(te => te.Subject)
                .Include(te => te.Teacher).ThenInclude(t => t.User)
                .Include(te => te.SubstituteTeacher).ThenInclude(st => st.User)
                .Include(te => te.CreatedBy)
                .FirstOrDefaultAsync(te => te.Id == id);

            return entry != null ? MapToDTO(entry) : null;
        }

        private static bool IsUserInTeacherRole(string? role) =>
            string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase);

        /// <summary>When <paramref name="teacherStaffId"/> is set, staff must exist, be active, and have User.Role Teacher.</summary>
        private async Task<string?> ValidateTimetableTeacherStaffAsync(Guid? teacherStaffId)
        {
            if (!teacherStaffId.HasValue)
                return null;

            var staff = await _context.Staff.AsNoTracking()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == teacherStaffId.Value);
            if (staff == null)
                return "Teacher (staff) not found.";
            if (staff.User == null || !staff.User.IsActive)
                return "Selected teacher account is inactive.";
            if (!IsUserInTeacherRole(staff.User.Role))
                return "Only users with the Teacher role can be assigned on the timetable.";
            return null;
        }

        public async Task<TimetableEntryDTO> CreateTimetableEntryAsync(CreateTimetableEntryDTO createTimetableEntryDto, Guid createdByUserId)
        {
            if (!createTimetableEntryDto.ScheduleDate.HasValue)
                throw new ArgumentException("ScheduleDate is required; timetable entries are stored per calendar day only.");

            var teacherErr = await ValidateTimetableTeacherStaffAsync(createTimetableEntryDto.TeacherId);
            if (teacherErr != null)
                throw new ArgumentException(teacherErr);

            var entry = new TimetableEntry
            {
                Id = Guid.NewGuid(),
                PeriodId = createTimetableEntryDto.PeriodId,
                ClassId = createTimetableEntryDto.ClassId,
                AcademicYearId = createTimetableEntryDto.AcademicYearId,
                SubjectId = createTimetableEntryDto.SubjectId,
                TeacherId = createTimetableEntryDto.TeacherId,
                Notes = createTimetableEntryDto.Notes ?? string.Empty,
                IsActive = createTimetableEntryDto.IsActive,
                ScheduleDate = createTimetableEntryDto.ScheduleDate.Value.Date,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.TimetableEntries.Add(entry);
            await _context.SaveChangesAsync();

            return await GetTimetableEntryByIdAsync(entry.Id) ?? throw new InvalidOperationException("Failed to create timetable entry");
        }

        public async Task<TimetableEntryDTO?> UpdateTimetableEntryAsync(Guid id, UpdateTimetableEntryDTO updateTimetableEntryDto)
        {
            var entry = await _context.TimetableEntries.FindAsync(id);
            if (entry == null) return null;

            if (updateTimetableEntryDto.SubjectId.HasValue)
                entry.SubjectId = updateTimetableEntryDto.SubjectId.Value;

            if (updateTimetableEntryDto.TeacherId.HasValue)
            {
                var teacherErr = await ValidateTimetableTeacherStaffAsync(updateTimetableEntryDto.TeacherId.Value);
                if (teacherErr != null)
                    throw new ArgumentException(teacherErr);
                entry.TeacherId = updateTimetableEntryDto.TeacherId.Value;
            }

            if (!string.IsNullOrEmpty(updateTimetableEntryDto.Notes))
                entry.Notes = updateTimetableEntryDto.Notes;

            if (updateTimetableEntryDto.IsActive.HasValue)
                entry.IsActive = updateTimetableEntryDto.IsActive.Value;

            if (updateTimetableEntryDto.ClearScheduleDate)
                throw new InvalidOperationException("Schedule date cannot be cleared; every entry must remain tied to a calendar day.");

            if (updateTimetableEntryDto.ScheduleDate.HasValue)
                entry.ScheduleDate = updateTimetableEntryDto.ScheduleDate.Value.Date;

            entry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetTimetableEntryByIdAsync(id);
        }

        public async Task<bool> DeleteTimetableEntryAsync(Guid id)
        {
            var entry = await _context.TimetableEntries.FindAsync(id);
            if (entry == null) return false;

            _context.TimetableEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TimetableEntryExistsAsync(Guid id)
        {
            return await _context.TimetableEntries.AnyAsync(te => te.Id == id);
        }

        public async Task<TimetableViewDTO> GetClassTimetableAsync(Guid classId)
        {
            var classEntity = await _context.Classes.FindAsync(classId);
            if (classEntity == null)
                throw new ArgumentException("Class not found", nameof(classId));

            var entries = await GetTimetableByClassAsync(classId);
            var entriesList = entries.ToList();

            var dayWiseEntries = entriesList
                .GroupBy(e => e.DayOfWeek)
                .ToDictionary(g => g.Key, g => g.OrderBy(e => e.OrderIndex).ThenBy(e => e.StartTime).ToList());

            return new TimetableViewDTO
            {
                ClassId = classId,
                ClassName = classEntity.Name,
                Entries = entriesList,
                DayWiseEntries = dayWiseEntries
            };
        }

        public async Task<WeeklyTimetableDTO> GetWeeklyTimetableAsync()
        {
            var classes = await _context.Classes.OrderBy(c => c.Name).ToListAsync();
            var periods = await _context.Periods
                .Where(p => p.IsActive)
                .OrderBy(p => p.DayOfWeek)
                .ThenBy(p => p.OrderIndex)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            var classTimetables = new List<TimetableViewDTO>();

            foreach (var classEntity in classes)
            {
                var classTimetable = await GetClassTimetableAsync(classEntity.Id);
                classTimetables.Add(classTimetable);
            }

            var dayNames = new Dictionary<int, string>
            {
                { 1, "Monday" }, { 2, "Tuesday" }, { 3, "Wednesday" },
                { 4, "Thursday" }, { 5, "Friday" }, { 6, "Saturday" }, { 7, "Sunday" }
            };

            return new WeeklyTimetableDTO
            {
                Classes = classTimetables,
                Periods = periods.Select(p => new PeriodDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    StartTime = p.StartTime,
                    EndTime = p.EndTime,
                    DayOfWeek = p.DayOfWeek,
                    DayName = dayNames.GetValueOrDefault(p.DayOfWeek, "Unknown"),
                    PeriodType = p.PeriodType,
                    OrderIndex = p.OrderIndex,
                    TimeRange = p.TimeRange,
                    DurationMinutes = p.DurationMinutes
                }).ToList(),
                DayNames = dayNames,
                WeekStartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1),
                WeekEndDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 7)
            };
        }

        public async Task<WeeklyTimetableDTO> GetWeeklyTimetableForUserAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Student)
                .Include(u => u.Staff)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new ArgumentException("User not found", nameof(userId));

            if (user.Student != null)
            {
                // Student - get only their class timetable
                var studentClassIds = await _context.ClassEnrollments
                    .Where(ce => ce.StudentId == user.Student.Id)
                    .Select(ce => ce.ClassId)
                    .ToListAsync();

                var classes = await _context.Classes
                    .Where(c => studentClassIds.Contains(c.Id))
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var classTimetables = new List<TimetableViewDTO>();
                foreach (var classEntity in classes)
                {
                    var classTimetable = await GetClassTimetableAsync(classEntity.Id);
                    classTimetables.Add(classTimetable);
                }

                var periods = await _context.Periods
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.DayOfWeek)
                    .ThenBy(p => p.OrderIndex)
                    .ThenBy(p => p.StartTime)
                    .ToListAsync();

                return new WeeklyTimetableDTO
                {
                    Classes = classTimetables,
                    Periods = periods.Select(MapPeriodToDTO).ToList(),
                    DayNames = GetDayNames(),
                    WeekStartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1),
                    WeekEndDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 7)
                };
            }
            else
            {
                // Staff or Admin - get all timetables
                return await GetWeeklyTimetableAsync();
            }
        }

        public async Task<bool> HasTeacherConflictAsync(TeacherConflictCheckDTO conflictCheck)
        {
            var (has, _) = await CheckTeacherConflictWithSummaryAsync(conflictCheck);
            return has;
        }

        public async Task<(bool HasConflict, string? BlockingSummary)> CheckTeacherConflictWithSummaryAsync(
            TeacherConflictCheckDTO conflictCheck)
        {
            var period = await _context.Periods.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == conflictCheck.PeriodId);
            if (period == null) return (false, null);

            static string FormatBlocker(TimetableEntry te)
            {
                var cn = te.Class?.Name ?? "Another class";
                var sn = te.Subject?.Name ?? "—";
                var p = te.Period;
                if (p == null) return $"{cn} · {sn}";
                return $"{cn} · {sn} · {p.StartTime:hh\\:mm}–{p.EndTime:hh\\:mm}";
            }

            // Dated substitute checks: exact period match.
            if (conflictCheck.SpecificDate.HasValue)
            {
                var queryDated = _context.TimetableEntries
                    .Include(te => te.Class)
                    .Include(te => te.Subject)
                    .Include(te => te.Period)
                    .Where(te => te.IsActive &&
                                 te.PeriodId == conflictCheck.PeriodId &&
                                 (te.TeacherId == conflictCheck.TeacherId ||
                                  (te.SubstituteTeacherId == conflictCheck.TeacherId &&
                                   te.SubstituteDate.HasValue &&
                                   te.SubstituteDate.Value.Date == conflictCheck.SpecificDate.Value.Date)));

                if (conflictCheck.ExcludeTimetableEntryId.HasValue)
                    queryDated = queryDated.Where(te => te.Id != conflictCheck.ExcludeTimetableEntryId.Value);

                var blocker = await queryDated.AsNoTracking().FirstOrDefaultAsync();
                return blocker != null ? (true, FormatBlocker(blocker)) : (false, null);
            }

            if (conflictCheck.ScheduleDateForConflict.HasValue)
            {
                var d = conflictCheck.ScheduleDateForConflict.Value.Date;
                var queryCal = _context.TimetableEntries
                    .Include(te => te.Period)
                    .Include(te => te.Class)
                    .Include(te => te.Subject)
                    .Where(te => te.IsActive &&
                                 te.TeacherId == conflictCheck.TeacherId &&
                                 te.Period != null &&
                                 te.Period.StartTime < period.EndTime &&
                                 te.Period.EndTime > period.StartTime &&
                                 te.ScheduleDate.HasValue &&
                                 te.ScheduleDate.Value.Date == d);

                if (conflictCheck.AcademicYearId.HasValue)
                    queryCal = queryCal.Where(te => te.AcademicYearId == conflictCheck.AcademicYearId.Value);

                if (conflictCheck.ExcludeTimetableEntryId.HasValue)
                    queryCal = queryCal.Where(te => te.Id != conflictCheck.ExcludeTimetableEntryId.Value);

                var blocker = await queryCal.AsNoTracking().FirstOrDefaultAsync();
                return blocker != null ? (true, FormatBlocker(blocker)) : (false, null);
            }

            return (false, null);
        }

        public async Task<string?> ValidateClassTeacherForFirstPeriodAsync(Guid periodId, Guid classId, Guid? teacherId, Guid academicYearId)
        {
            var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == classId);
            if (cls?.TeacherId == null) return null;

            var period = await _context.Periods.AsNoTracking().FirstOrDefaultAsync(p => p.Id == periodId);
            if (period == null) return null;

            var firstRegular = await _context.Periods.AsNoTracking()
                .Where(p => p.IsActive &&
                            p.DayOfWeek == period.DayOfWeek &&
                            (p.AcademicYearId == null || p.AcademicYearId == academicYearId) &&
                            string.Equals(p.PeriodType, "Regular", StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.OrderIndex)
                .ThenBy(p => p.StartTime)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (firstRegular == Guid.Empty || firstRegular != periodId) return null;

            if (!teacherId.HasValue || teacherId.Value != cls.TeacherId.Value)
            {
                return "The first teaching period of each day for this class must be assigned to the class teacher (the teacher assigned to this class in class settings).";
            }

            var classTeacher = await _context.Staff.AsNoTracking()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == cls.TeacherId.Value);
            if (classTeacher?.User != null && !IsUserInTeacherRole(classTeacher.User.Role))
            {
                return "The class teacher in class settings must have the Teacher role to be used on the timetable.";
            }

            return null;
        }

        public async Task<IEnumerable<TimetableEntryDTO>> GetTeacherConflictsAsync(Guid teacherId, Guid periodId, DateTime? specificDate = null)
        {
            var query = _context.TimetableEntries
                .Include(te => te.Period)
                .Include(te => te.Class)
                .Include(te => te.Subject)
                .Include(te => te.Teacher).ThenInclude(t => t.User)
                .Include(te => te.SubstituteTeacher).ThenInclude(st => st.User)
                .Include(te => te.CreatedBy)
                .Where(te => te.IsActive &&
                            te.PeriodId == periodId &&
                            (te.TeacherId == teacherId ||
                             (te.SubstituteTeacherId == teacherId &&
                              te.SubstituteDate.HasValue &&
                              (!specificDate.HasValue ||
                               te.SubstituteDate.Value.Date == specificDate.Value.Date))));

            var entries = await query.ToListAsync();
            return entries.Select(MapToDTO);
        }

        public async Task<bool> AssignSubstituteTeacherAsync(AssignSubstituteDTO assignSubstituteDto, Guid assignedByUserId)
        {
            var entry = await _context.TimetableEntries.FindAsync(assignSubstituteDto.TimetableEntryId);
            if (entry == null) return false;

            var subErr = await ValidateTimetableTeacherStaffAsync(assignSubstituteDto.SubstituteTeacherId);
            if (subErr != null)
                throw new ArgumentException(subErr);

            // Check if substitute teacher has conflict
            var conflictCheck = new TeacherConflictCheckDTO
            {
                TeacherId = assignSubstituteDto.SubstituteTeacherId,
                PeriodId = entry.PeriodId,
                ExcludeTimetableEntryId = entry.Id,
                SpecificDate = assignSubstituteDto.SubstituteDate
            };

            if (await HasTeacherConflictAsync(conflictCheck))
            {
                return false; // Substitute teacher has conflict
            }

            entry.SubstituteTeacherId = assignSubstituteDto.SubstituteTeacherId;
            entry.SubstituteDate = assignSubstituteDto.SubstituteDate;
            entry.Notes = !string.IsNullOrEmpty(assignSubstituteDto.Notes)
                ? assignSubstituteDto.Notes
                : entry.Notes;
            entry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveSubstituteTeacherAsync(Guid timetableEntryId, DateTime substituteDate)
        {
            var entry = await _context.TimetableEntries.FindAsync(timetableEntryId);
            if (entry == null ||
                !entry.SubstituteDate.HasValue ||
                entry.SubstituteDate.Value.Date != substituteDate.Date)
                return false;

            entry.SubstituteTeacherId = null;
            entry.SubstituteDate = null;
            entry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TimetableEntryDTO>> GetSubstituteAssignmentsAsync(DateTime date)
        {
            var entries = await _context.TimetableEntries
                .Include(te => te.Period)
                .Include(te => te.Class)
                .Include(te => te.Subject)
                .Include(te => te.Teacher).ThenInclude(t => t.User)
                .Include(te => te.SubstituteTeacher).ThenInclude(st => st.User)
                .Include(te => te.CreatedBy)
                .Where(te => te.IsActive &&
                            te.SubstituteDate.HasValue &&
                            te.SubstituteDate.Value.Date == date.Date)
                .OrderBy(te => te.Period.OrderIndex)
                .ThenBy(te => te.Period.StartTime)
                .ToListAsync();

            return entries.Select(MapToDTO);
        }

        public async Task<IEnumerable<TimetableEntryDTO>> GetTeacherSubstituteHistoryAsync(Guid teacherId)
        {
            var entries = await _context.TimetableEntries
                .Include(te => te.Period)
                .Include(te => te.Class)
                .Include(te => te.Subject)
                .Include(te => te.Teacher).ThenInclude(t => t.User)
                .Include(te => te.SubstituteTeacher).ThenInclude(st => st.User)
                .Include(te => te.CreatedBy)
                .Where(te => te.SubstituteTeacherId == teacherId)
                .OrderByDescending(te => te.SubstituteDate)
                .ToListAsync();

            return entries.Select(MapToDTO);
        }

        public async Task<bool> CreateClassTimetableAsync(Guid classId, List<CreateTimetableEntryDTO> entries, Guid createdByUserId)
        {
            // Remove existing entries for the class
            var existingEntries = await _context.TimetableEntries
                .Where(te => te.ClassId == classId)
                .ToListAsync();

            _context.TimetableEntries.RemoveRange(existingEntries);

            foreach (var dto in entries)
            {
                if (!dto.ScheduleDate.HasValue)
                    throw new ArgumentException("Each timetable row must include ScheduleDate.");
                var err = await ValidateTimetableTeacherStaffAsync(dto.TeacherId);
                if (err != null)
                    throw new ArgumentException(err);
            }

            // Add new entries
            var newEntries = entries.Select(dto => new TimetableEntry
            {
                Id = Guid.NewGuid(),
                PeriodId = dto.PeriodId,
                ClassId = classId,
                AcademicYearId = dto.AcademicYearId,
                SubjectId = dto.SubjectId,
                TeacherId = dto.TeacherId,
                Notes = dto.Notes ?? string.Empty,
                IsActive = dto.IsActive,
                ScheduleDate = dto.ScheduleDate!.Value.Date,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.TimetableEntries.AddRange(newEntries);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CopyTimetableAsync(Guid fromClassId, Guid toClassId, Guid createdByUserId)
        {
            var sourceEntries = await _context.TimetableEntries
                .Where(te => te.ClassId == fromClassId && te.IsActive)
                .ToListAsync();

            if (!sourceEntries.Any()) return false;

            // Remove existing entries for target class
            var existingEntries = await _context.TimetableEntries
                .Where(te => te.ClassId == toClassId)
                .ToListAsync();

            _context.TimetableEntries.RemoveRange(existingEntries);

            // Copy entries to target class
            var copiedEntries = sourceEntries.Select(se => new TimetableEntry
            {
                Id = Guid.NewGuid(),
                PeriodId = se.PeriodId,
                ClassId = toClassId,
                AcademicYearId = se.AcademicYearId,
                SubjectId = se.SubjectId,
                TeacherId = se.TeacherId,
                Notes = se.Notes,
                IsActive = se.IsActive,
                ScheduleDate = se.ScheduleDate,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.TimetableEntries.AddRange(copiedEntries);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> ValidateCopyDayTargetDateAsync(Guid academicYearId, DateTime toDate)
        {
            var d = toDate.Date;
            var ay = await _context.AcademicYears.AsNoTracking().FirstOrDefaultAsync(a => a.Id == academicYearId);
            if (ay == null)
                return "Invalid academic year.";

            if (d < ay.StartDate.Date || d > ay.EndDate.Date)
                return "Target date must fall within the selected academic session (start–end dates of that year).";

            var holidayName = await _context.Holidays.AsNoTracking()
                .Where(h => h.AcademicYearId == academicYearId && h.IsActive &&
                            h.StartDate.Date <= d && h.EndDate.Date >= d)
                .Select(h => h.Name)
                .FirstOrDefaultAsync();
            if (holidayName != null)
                return $"Cannot copy onto a holiday ({holidayName}).";

            var jsDow = (int)d.DayOfWeek; // 0 = Sunday … matches WeekOff.DayOfWeek
            var weekOff = await _context.WeekOffs.AsNoTracking()
                .Where(w => w.AcademicYearId == academicYearId && w.IsActive && w.DayOfWeek == jsDow)
                .ToListAsync();

            foreach (var w in weekOff)
            {
                if (w.EffectiveFrom.Date > d)
                    continue;
                if (w.EffectiveTo.HasValue && w.EffectiveTo.Value.Date < d)
                    continue;
                return $"Cannot copy onto a week off ({w.Name}).";
            }

            return null;
        }

        public async Task<int> CopyTimetableDayAsync(CopyTimetableDayDTO dto, Guid createdByUserId)
        {
            var from = dto.FromDate.Date;
            var to = dto.ToDate.Date;
            if (from == to) return 0;

            var ay = dto.AcademicYearId;
            var dowFrom = ToApiDayOfWeek(from.DayOfWeek);
            var dowTo = ToApiDayOfWeek(to.DayOfWeek);

            if (dto.ReplaceExistingOnTargetDate)
            {
                var delQuery = _context.TimetableEntries.Where(te =>
                    te.AcademicYearId == ay &&
                    te.ScheduleDate.HasValue &&
                    te.ScheduleDate.Value.Date == to);
                if (dto.ClassId.HasValue)
                    delQuery = delQuery.Where(te => te.ClassId == dto.ClassId.Value);
                _context.TimetableEntries.RemoveRange(await delQuery.ToListAsync());
            }

            var sourceQuery = _context.TimetableEntries
                .Include(te => te.Period)
                .Where(te => te.IsActive &&
                             te.AcademicYearId == ay &&
                             te.ScheduleDate.HasValue &&
                             te.ScheduleDate.Value.Date == from);

            if (dto.ClassId.HasValue)
                sourceQuery = sourceQuery.Where(te => te.ClassId == dto.ClassId.Value);

            var sources = await sourceQuery.ToListAsync();

            // All active periods for target weekday (any year). Prefer matching AcademicYearId when mapping.
            var periodsTo = await _context.Periods.AsNoTracking()
                .Where(p => p.IsActive && p.DayOfWeek == dowTo)
                .OrderBy(p => p.OrderIndex)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            var created = 0;
            foreach (var se in sources)
            {
                if (se.Period == null)
                    continue;

                Guid newPeriodId;
                if (dowFrom == dowTo)
                {
                    // Same weekday: keep the same period row (entries already point at the correct template).
                    newPeriodId = se.PeriodId;
                }
                else
                {
                    var pt = MatchTargetPeriodForCopy(se.Period, periodsTo, ay);
                    if (pt == null)
                        continue;
                    newPeriodId = pt.Id;
                }

                _context.TimetableEntries.Add(new TimetableEntry
                {
                    Id = Guid.NewGuid(),
                    PeriodId = newPeriodId,
                    ClassId = se.ClassId,
                    AcademicYearId = ay,
                    SubjectId = se.SubjectId,
                    TeacherId = se.TeacherId,
                    Notes = se.Notes,
                    IsActive = se.IsActive,
                    ScheduleDate = to,
                    CreatedByUserId = createdByUserId,
                    CreatedAt = DateTime.UtcNow
                });
                created++;
            }

            await _context.SaveChangesAsync();
            return created;
        }

        /// <summary>Map a source day period to the target weekday’s period (order/type first, then times). Prefers same academic year.</summary>
        private static Period? MatchTargetPeriodForCopy(Period pf, List<Period> periodsTo, Guid academicYearId)
        {
            static bool TypeMatch(Period a, Period b) =>
                string.Equals(a.PeriodType ?? "Regular", b.PeriodType ?? "Regular", StringComparison.OrdinalIgnoreCase);

            var byOrder = periodsTo.Where(x => x.OrderIndex == pf.OrderIndex && TypeMatch(x, pf)).ToList();
            var pt = byOrder.FirstOrDefault(x => x.AcademicYearId == academicYearId)
                     ?? byOrder.FirstOrDefault(x => x.AcademicYearId == null)
                     ?? byOrder.FirstOrDefault();

            if (pt != null)
                return pt;

            var byTime = periodsTo.Where(x => x.StartTime == pf.StartTime && x.EndTime == pf.EndTime).ToList();
            return byTime.FirstOrDefault(x => x.AcademicYearId == academicYearId)
                   ?? byTime.FirstOrDefault(x => x.AcademicYearId == null)
                   ?? byTime.FirstOrDefault();
        }

        /// <summary>Period model uses Monday=1 … Sunday=7.</summary>
        private static int ToApiDayOfWeek(DayOfWeek dow) =>
            dow == DayOfWeek.Sunday ? 7 : (int)dow;

        private static TimetableEntryDTO MapToDTO(TimetableEntry entry)
        {
            var dayNames = new[] { "", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            return new TimetableEntryDTO
            {
                Id = entry.Id,
                PeriodId = entry.PeriodId,
                PeriodName = entry.Period?.Name ?? "Unknown",
                StartTime = entry.Period?.StartTime ?? TimeSpan.Zero,
                EndTime = entry.Period?.EndTime ?? TimeSpan.Zero,
                TimeRange = entry.Period?.TimeRange ?? "Unknown",
                DayOfWeek = entry.Period?.DayOfWeek ?? 0,
                DayName = entry.Period != null ? dayNames[entry.Period.DayOfWeek] : "Unknown",
                PeriodType = entry.Period?.PeriodType ?? "Regular",
                OrderIndex = entry.Period?.OrderIndex ?? 0,
                ClassId = entry.ClassId,
                ClassName = entry.Class?.Name ?? "Unknown",
                SubjectId = entry.SubjectId,
                SubjectName = entry.Subject?.Name ?? "No Subject",
                TeacherId = entry.TeacherId,
                TeacherName = entry.Teacher?.User?.FullName ?? "No Teacher",
                SubstituteTeacherId = entry.SubstituteTeacherId,
                SubstituteTeacherName = entry.SubstituteTeacher?.User?.FullName ?? "",
                SubstituteDate = entry.SubstituteDate,
                Notes = entry.Notes,
                IsActive = entry.IsActive,
                AcademicYearId = entry.AcademicYearId,
                ScheduleDate = entry.ScheduleDate,
                CreatedByName = entry.CreatedBy?.FullName ?? "Unknown",
                CreatedAt = entry.CreatedAt,
                UpdatedAt = entry.UpdatedAt,
                EffectiveTeacherName = entry.EffectiveTeacher?.User?.FullName ?? "No Teacher",
                HasSubstitute = entry.HasSubstitute,
                IsSubstituteActive = entry.IsSubstituteActive
            };
        }

        private static PeriodDTO MapPeriodToDTO(Period period)
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

        private static Dictionary<int, string> GetDayNames()
        {
            return new Dictionary<int, string>
            {
                { 1, "Monday" }, { 2, "Tuesday" }, { 3, "Wednesday" },
                { 4, "Thursday" }, { 5, "Friday" }, { 6, "Saturday" }, { 7, "Sunday" }
            };
        }
    }
}