using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Services;

namespace sms.api.Services;

/// <summary>
/// Domain notification helpers — resolve recipients and fire orchestrator.
/// </summary>
public static class NotificationEvents
{
    public static async Task NotifyUsersAsync(
        INotificationOrchestrator? orch,
        IEnumerable<Guid> userIds,
        string title,
        string body,
        string type,
        string category,
        NotificationChannels channels,
        Guid? entityId = null,
        string? entityType = null,
        string severity = "info",
        string? smsBody = null,
        string? idempotencyKey = null,
        bool includeAdmins = false,
        Dictionary<string, string>? data = null)
    {
        if (orch == null) return;
        var ids = userIds?.Where(id => id != Guid.Empty).Distinct().ToList() ?? new List<Guid>();
        if (ids.Count == 0 && !includeAdmins) return;

        // Await so inbox rows exist before the domain API returns (bell can refresh immediately).
        await orch.NotifyAsync(new NotificationRequest
        {
            UserIds = ids,
            IncludeAdmins = includeAdmins,
            Title = title,
            Body = body,
            SmsBody = smsBody ?? body,
            Type = type,
            Category = category,
            Channels = channels,
            EntityId = entityId,
            EntityType = entityType,
            Severity = severity,
            IdempotencyKey = idempotencyKey,
            Data = data ?? new Dictionary<string, string>
            {
                ["type"] = type,
                ["entityId"] = entityId?.ToString() ?? ""
            }
        });
    }

    public static async Task<Guid?> GetStudentUserIdAsync(ApplicationDbContext db, Guid studentId)
    {
        return await db.Students.AsNoTracking()
            .Where(s => s.Id == studentId)
            .Select(s => (Guid?)s.UserId)
            .FirstOrDefaultAsync();
    }

    public static async Task<Guid?> GetStaffUserIdAsync(ApplicationDbContext db, Guid staffId)
    {
        return await db.Staff.AsNoTracking()
            .Where(s => s.Id == staffId)
            .Select(s => (Guid?)s.UserId)
            .FirstOrDefaultAsync();
    }

    public static async Task<List<Guid>> GetClassTeacherUserIdsAsync(ApplicationDbContext db, Guid? classId)
    {
        if (!classId.HasValue) return new List<Guid>();
        return await db.Classes.AsNoTracking()
            .Where(c => c.Id == classId.Value && c.TeacherId != null)
            .Join(db.Staff.AsNoTracking(), c => c.TeacherId, s => s.Id, (c, s) => s.UserId)
            .Where(uid => uid != Guid.Empty)
            .Distinct()
            .ToListAsync();
    }

    public static async Task<List<Guid>> GetStaffLeaveApproverUserIdsAsync(ApplicationDbContext db, Guid staffId)
    {
        return await db.StaffLeaveApprovers.AsNoTracking()
            .Where(a => a.StaffId == staffId && a.IsActive)
            .Join(db.Staff.AsNoTracking(), a => a.ApproverId, s => s.Id, (a, s) => s.UserId)
            .Where(uid => uid != Guid.Empty)
            .Distinct()
            .ToListAsync();
    }

    public static async Task<List<Guid>> GetAdminUserIdsAsync(ApplicationDbContext db)
    {
        return await db.Users.AsNoTracking()
            .Where(u => u.IsActive && (u.Role == "Admin" || u.Role == "admin"))
            .Select(u => u.Id)
            .Take(50)
            .ToListAsync();
    }

    public static async Task<List<Guid>> GetActiveStudentUserIdsInClassAsync(ApplicationDbContext db, Guid classId)
    {
        return await db.ClassEnrollments.AsNoTracking()
            .Where(e => e.ClassId == classId && e.IsActive)
            .Join(db.Students.AsNoTracking(), e => e.StudentId, s => s.Id, (e, s) => s.UserId)
            .Where(uid => uid != Guid.Empty)
            .Distinct()
            .ToListAsync();
    }
}
