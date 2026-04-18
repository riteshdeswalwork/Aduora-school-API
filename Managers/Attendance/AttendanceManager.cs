using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Attendance;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using AttendanceEntity = sms.api.Models.Attendance;

namespace sms.api.Managers.Attendance
{
    public class AttendanceManager : GenericManager<AttendanceEntity>, IAttendanceManager
    {
        public AttendanceManager(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<AttendanceEntity>> GetAllAsync()
        {
            return await _dbSet.Include(a => a.Student).Include(a => a.Course).ToListAsync();
        }

        public override async Task<AttendanceEntity> GetByIdAsync(Guid id)
        {
            return await _dbSet.Include(a => a.Student).Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<AttendanceEntity>> GetAttendanceByStudentIdAsync(Guid studentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbSet.Include(a => a.Course).Where(a => a.StudentId == studentId);

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);

            return await query.OrderByDescending(a => a.Date).ToListAsync();
        }

        public async Task<IEnumerable<AttendanceEntity>> GetAttendanceByCourseIdAsync(Guid courseId, DateTime? date = null)
        {
            var query = _dbSet.Include(a => a.Student).Where(a => a.CourseId == courseId);

            if (date.HasValue)
                query = query.Where(a => a.Date.Date == date.Value.Date);

            return await query.OrderByDescending(a => a.Date).ToListAsync();
        }

        public async Task<bool> MarkAttendanceAsync(Guid studentId, Guid courseId, DateTime date, bool isPresent, string? remarks = null)
        {
            // Check if attendance already exists for this student, course, and date
            var existingAttendance = await _dbSet.FirstOrDefaultAsync(a =>
                a.StudentId == studentId &&
                a.CourseId == courseId &&
                a.Date.Date == date.Date);

            if (existingAttendance != null)
            {
                // Update existing attendance
                existingAttendance.IsPresent = isPresent;
                existingAttendance.Remarks = remarks;
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                // Create new attendance record
                var attendance = new AttendanceEntity
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    CourseId = courseId,
                    Date = date,
                    IsPresent = isPresent,
                    Remarks = remarks,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbSet.AddAsync(attendance);
                await _context.SaveChangesAsync();
                return true;
            }
        }
    }
}
