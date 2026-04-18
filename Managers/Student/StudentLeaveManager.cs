using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Student;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Managers.Student
{
    public class StudentLeaveManager : GenericManager<StudentLeave>, IStudentLeaveManager
    {

        public StudentLeaveManager(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<StudentLeave>> GetAllAsync()
        {
            return await _dbSet
                .Include(sl => sl.Student)
                    .ThenInclude(s => s.User)
                .Include(sl => sl.Class)
                .Include(sl => sl.ApprovedBy)
                .ToListAsync();
        }

        public override async Task<StudentLeave> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(sl => sl.Student)
                    .ThenInclude(s => s.User)
                .Include(sl => sl.Class)
                .Include(sl => sl.ApprovedBy)
                .FirstOrDefaultAsync(sl => sl.Id == id);
        }

        public async Task<IEnumerable<StudentLeave>> GetLeavesByStudentIdAsync(Guid studentId)
        {
            return await _dbSet
                .Include(sl => sl.Student)
                    .ThenInclude(s => s.User)
                .Include(sl => sl.Class)
                .Include(sl => sl.ApprovedBy)
                .Where(sl => sl.StudentId == studentId)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentLeave>> GetLeavesByClassIdAsync(Guid classId)
        {
            return await _dbSet
                .Include(sl => sl.Student)
                    .ThenInclude(s => s.User)
                .Include(sl => sl.Class)
                .Include(sl => sl.ApprovedBy)
                .Where(sl => sl.ClassId == classId)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentLeave>> GetPendingLeavesForTeacherAsync(Guid teacherId)
        {
            // Get all classes assigned to this teacher
            var teacherClasses = await _context.Classes
                .Where(c => c.TeacherId == teacherId)
                .Select(c => c.Id)
                .ToListAsync();

            // Get all pending leaves for students in these classes
            return await _dbSet
                .Include(sl => sl.Student)
                    .ThenInclude(s => s.User)
                .Include(sl => sl.Class)
                .Where(sl => teacherClasses.Contains(sl.ClassId.Value) && sl.Status == "Pending")
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ApproveLeaveAsync(Guid leaveId, Guid approverId)
        {
            var leave = await GetByIdAsync(leaveId);
            if (leave == null)
                return false;

            leave.Status = "Approved";
            leave.ApprovedById = approverId;
            leave.ApprovedAt = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectLeaveAsync(Guid leaveId, Guid approverId, string reason)
        {
            var leave = await GetByIdAsync(leaveId);
            if (leave == null)
                return false;

            leave.Status = "Rejected";
            leave.RejectionReason = reason;
            leave.ApprovedById = approverId;
            leave.ApprovedAt = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<StudentLeave> CreateStudentLeaveAsync(CreateStudentLeaveDTO leaveDto)
        {
            // Get the student's class if not provided
            if (!leaveDto.ClassId.HasValue)
            {
                var classEnrollment = await _context.ClassEnrollments
                    .Where(ce => ce.StudentId == leaveDto.StudentId && ce.IsActive)
                    .OrderByDescending(ce => ce.EnrollmentDate)
                    .FirstOrDefaultAsync();

                if (classEnrollment != null)
                {
                    leaveDto.ClassId = classEnrollment.ClassId;
                }
            }

            var leave = new StudentLeave
            {
                Id = Guid.NewGuid(),
                StudentId = leaveDto.StudentId,
                ClassId = leaveDto.ClassId,
                StartDate = leaveDto.StartDate,
                EndDate = leaveDto.EndDate,
                Reason = leaveDto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await CreateAsync(leave);
        }
    }
}
