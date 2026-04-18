using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Managers.Academic
{
    public class ClassAttendanceManager : GenericManager<ClassAttendance>, IClassAttendanceManager
    {

        public ClassAttendanceManager(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<ClassAttendance>> GetAllAsync()
        {
            return await _dbSet
                .Include(ca => ca.Class)
                .Include(ca => ca.CreatedBy)
                .OrderByDescending(ca => ca.Date)
                .ToListAsync();
        }

        public override async Task<ClassAttendance> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(ca => ca.Class)
                .Include(ca => ca.CreatedBy)
                .FirstOrDefaultAsync(ca => ca.Id == id);
        }

        public async Task<ClassAttendance> GetClassAttendanceByIdWithRecordsAsync(Guid id)
        {
            return await _dbSet
                .Include(ca => ca.Class)
                .Include(ca => ca.CreatedBy)
                .Include(ca => ca.AttendanceRecords)
                    .ThenInclude(ar => ar.Student)
                        .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(ca => ca.Id == id);
        }

        public async Task<IEnumerable<ClassAttendance>> GetClassAttendancesByClassIdAsync(Guid classId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbSet
                .Include(ca => ca.Class)
                .Include(ca => ca.CreatedBy)
                .Include(ca => ca.AttendanceRecords)
                .Where(ca => ca.ClassId == classId);

            if (fromDate.HasValue)
                query = query.Where(ca => ca.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(ca => ca.Date <= toDate.Value.Date);

            return await query.OrderByDescending(ca => ca.Date).ToListAsync();
        }

        public async Task<IEnumerable<ClassAttendance>> GetClassAttendancesByTeacherIdAsync(Guid teacherId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var classIds = await _context.Classes
                .Where(c => c.TeacherId == teacherId)
                .Select(c => c.Id)
                .ToListAsync();

            var query = _dbSet
                .Include(ca => ca.Class)
                .Include(ca => ca.CreatedBy)
                .Include(ca => ca.AttendanceRecords)
                .Where(ca => classIds.Contains(ca.ClassId));

            if (fromDate.HasValue)
                query = query.Where(ca => ca.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(ca => ca.Date <= toDate.Value.Date);

            return await query.OrderByDescending(ca => ca.Date).ToListAsync();
        }

        public async Task<IEnumerable<ClassAttendanceRecord>> GetAttendanceRecordsByStudentIdAsync(Guid studentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.ClassAttendanceRecords
                .Include(ar => ar.ClassAttendance)
                    .ThenInclude(ca => ca.Class)
                .Where(ar => ar.StudentId == studentId);

            if (fromDate.HasValue)
                query = query.Where(ar => ar.ClassAttendance.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(ar => ar.ClassAttendance.Date <= toDate.Value.Date);

            return await query.OrderByDescending(ar => ar.ClassAttendance.Date).ToListAsync();
        }

        public async Task<ClassAttendance> CreateClassAttendanceWithRecordsAsync(CreateClassAttendanceDTO attendanceDto)
        {
            // Check if attendance already exists for this class and date
            var existingAttendance = await _dbSet
                .FirstOrDefaultAsync(ca => ca.ClassId == attendanceDto.ClassId && ca.Date.Date == attendanceDto.Date.Date);

            if (existingAttendance != null)
            {
                throw new InvalidOperationException("Attendance for this class on this date already exists.");
            }

            // Create new attendance
            var classAttendance = new ClassAttendance
            {
                Id = Guid.NewGuid(),
                ClassId = attendanceDto.ClassId,
                Date = attendanceDto.Date,
                Remarks = attendanceDto.Remarks,
                CreatedAt = DateTime.UtcNow,
                CreatedById = attendanceDto.CreatedById
            };

            // Add attendance records
            foreach (var recordDto in attendanceDto.AttendanceRecords)
            {
                var record = new ClassAttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    ClassAttendanceId = classAttendance.Id,
                    StudentId = recordDto.StudentId,
                    Status = recordDto.Status,
                    Remarks = recordDto.Remarks
                };

                classAttendance.AttendanceRecords.Add(record);
            }

            await _dbSet.AddAsync(classAttendance);
            await _context.SaveChangesAsync();

            return await GetClassAttendanceByIdWithRecordsAsync(classAttendance.Id);
        }

        public async Task<ClassAttendanceRecord?> GetAttendanceRecordWithClassAsync(Guid recordId)
        {
            return await _context.ClassAttendanceRecords
                .AsNoTracking()
                .Include(ar => ar.ClassAttendance)
                .FirstOrDefaultAsync(ar => ar.Id == recordId);
        }

        public async Task<bool> UpdateAttendanceRecordAsync(Guid recordId, string status, string remarks)
        {
            var record = await _context.ClassAttendanceRecords.FindAsync(recordId);
            if (record == null)
                return false;

            record.Status = status;
            record.Remarks = remarks;

            // Update the parent attendance's UpdatedAt field
            var attendance = await _dbSet.FindAsync(record.ClassAttendanceId);
            if (attendance != null)
            {
                attendance.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
