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
using sms.api.Services;

namespace sms.api.Managers.Student
{
    public class StudentLeaveManager : GenericManager<StudentLeave>, IStudentLeaveManager
    {
        private readonly INotificationOrchestrator? _notify;

        public StudentLeaveManager(ApplicationDbContext context, INotificationOrchestrator? notify = null) : base(context)
        {
            _notify = notify;
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
            var teacherClasses = await _context.Classes
                .Where(c => c.TeacherId == teacherId)
                .Select(c => c.Id)
                .ToListAsync();

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

            var userId = leave.Student?.UserId ?? await NotificationEvents.GetStudentUserIdAsync(_context, leave.StudentId);
            if (userId.HasValue)
            {
                await NotificationEvents.NotifyUsersAsync(
                    _notify,
                    new[] { userId.Value },
                    "Leave approved",
                    $"Your leave from {leave.StartDate:dd MMM} to {leave.EndDate:dd MMM} was approved.",
                    "leave.approved",
                    "approvals",
                    NotificationChannels.All,
                    leave.Id,
                    "StudentLeave",
                    "success",
                    idempotencyKey: $"leave.approved:{leave.Id}");
            }
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

            var userId = leave.Student?.UserId ?? await NotificationEvents.GetStudentUserIdAsync(_context, leave.StudentId);
            if (userId.HasValue)
            {
                await NotificationEvents.NotifyUsersAsync(
                    _notify,
                    new[] { userId.Value },
                    "Leave rejected",
                    $"Your leave request was rejected. {reason}",
                    "leave.rejected",
                    "approvals",
                    NotificationChannels.All,
                    leave.Id,
                    "StudentLeave",
                    "warning",
                    idempotencyKey: $"leave.rejected:{leave.Id}");
            }
            return true;
        }

        public async Task<StudentLeave> CreateStudentLeaveAsync(CreateStudentLeaveDTO leaveDto)
        {
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

            var created = await CreateAsync(leave);

            var teachers = await NotificationEvents.GetClassTeacherUserIdsAsync(_context, leave.ClassId);
            var admins = await NotificationEvents.GetAdminUserIdsAsync(_context);
            var recipients = teachers.Concat(admins).Distinct().ToList();
            await NotificationEvents.NotifyUsersAsync(
                _notify,
                recipients,
                "New student leave request",
                $"A student leave request needs approval ({leave.StartDate:dd MMM} – {leave.EndDate:dd MMM}).",
                "leave.created",
                "approvals",
                NotificationChannels.InApp | NotificationChannels.Push | NotificationChannels.Email,
                leave.Id,
                "StudentLeave",
                "info");

            return created;
        }
    }
}
