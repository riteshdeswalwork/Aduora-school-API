using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services;

namespace sms.api.Managers.Shared
{
    public class AnnouncementManager : IAnnouncementManager
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationOrchestrator? _notify;

        public AnnouncementManager(ApplicationDbContext context, INotificationOrchestrator? notify = null)
        {
            _context = context;
            _notify = notify;
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            // Admin list: show current year when header set; otherwise all years
            return await _context.Announcements
                .AsNoTracking()
                .Include(a => a.Creator)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetPublishedAsync()
        {
            return await _context.Announcements
                .AsNoTracking()
                .Include(a => a.Creator)
                .Where(a => a.IsPublished &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.UtcNow))
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetFeaturedAsync()
        {
            return await _context.Announcements
                .AsNoTracking()
                .Include(a => a.Creator)
                .Where(a => a.IsPublished && a.IsFeatured &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.UtcNow))
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }

        public async Task<Announcement?> GetByIdAsync(Guid id)
        {
            return await _context.Announcements
                .IgnoreQueryFilters()
                .Include(a => a.Creator)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Announcement> CreateAsync(Announcement announcement)
        {
            // Avoid EF trying to insert null navigations as new rows
            announcement.Id = Guid.NewGuid();
            announcement.CreatedAt = DateTime.UtcNow;
            announcement.UpdatedAt = null;
            announcement.Creator = null;
            announcement.AcademicYear = null!;

            if (announcement.PublishDate == default)
                announcement.PublishDate = DateTime.UtcNow;

            _context.Announcements.Add(announcement);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException($"Could not save announcement: {detail}", ex);
            }

            if (announcement.IsPublished)
                await NotifyPublishedAsync(announcement);

            return announcement;
        }

        public async Task<Announcement> UpdateAsync(Announcement announcement)
        {
            var existing = await _context.Announcements
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == announcement.Id);

            if (existing == null)
                throw new InvalidOperationException("Announcement not found.");

            var wasPublished = existing.IsPublished;

            existing.Title = announcement.Title;
            existing.Content = announcement.Content;
            existing.Priority = announcement.Priority;
            existing.Category = announcement.Category;
            existing.PublishDate = announcement.PublishDate == default ? existing.PublishDate : announcement.PublishDate;
            existing.ExpiryDate = announcement.ExpiryDate;
            existing.IsPublished = announcement.IsPublished;
            existing.IsFeatured = announcement.IsFeatured;
            existing.ImageUrl = announcement.ImageUrl ?? string.Empty;
            existing.AcademicYearId = announcement.AcademicYearId;
            existing.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException($"Could not update announcement: {detail}", ex);
            }

            if (!wasPublished && existing.IsPublished)
                await NotifyPublishedAsync(existing);

            return existing;
        }

        public async Task<Announcement?> SetPublishedAsync(Guid id, bool isPublished)
        {
            var existing = await _context.Announcements
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == id);
            if (existing == null) return null;

            var wasPublished = existing.IsPublished;
            existing.IsPublished = isPublished;
            existing.UpdatedAt = DateTime.UtcNow;
            if (isPublished && existing.PublishDate == default)
                existing.PublishDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (!wasPublished && isPublished)
                await NotifyPublishedAsync(existing);

            return existing;
        }

        private async Task NotifyPublishedAsync(Announcement announcement)
        {
            var userIds = await _context.Users.AsNoTracking()
                .Where(u => u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            var body = announcement.Content ?? "A new announcement has been published.";
            if (body.Length > 500) body = body[..500];

            var priority = announcement.Priority ?? "Normal";
            var isHigh = priority.Equals("High", StringComparison.OrdinalIgnoreCase)
                         || priority.Equals("Urgent", StringComparison.OrdinalIgnoreCase);

            var channels = NotificationChannels.InApp | NotificationChannels.Push | NotificationChannels.Email;
            if (isHigh) channels |= NotificationChannels.Sms;

            await NotificationEvents.NotifyUsersAsync(
                _notify,
                userIds,
                announcement.Title ?? "New Announcement",
                body,
                "announcement.published",
                "general",
                channels,
                announcement.Id,
                "Announcement",
                severity: isHigh ? "warning" : "info",
                smsBody: TruncateSms($"{announcement.Title}: {body}"),
                idempotencyKey: $"announcement.published:{announcement.Id}",
                includeAdmins: true,
                data: new Dictionary<string, string>
                {
                    ["type"] = "announcement.published",
                    ["announcementId"] = announcement.Id.ToString(),
                    ["priority"] = priority
                });
        }

        private static string TruncateSms(string s)
        {
            s = (s ?? "").Trim();
            return s.Length <= 160 ? s : s[..159] + "…";
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var announcement = await _context.Announcements
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == id);
            if (announcement == null)
                return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Announcement>> GetByCategoryAsync(string category)
        {
            return await _context.Announcements
                .AsNoTracking()
                .Include(a => a.Creator)
                .Where(a => a.IsPublished && a.Category == category &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.UtcNow))
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetByPriorityAsync(string priority)
        {
            return await _context.Announcements
                .AsNoTracking()
                .Include(a => a.Creator)
                .Where(a => a.IsPublished && a.Priority == priority &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.UtcNow))
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }
    }
}
