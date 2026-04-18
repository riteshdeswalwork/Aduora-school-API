using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Services;

namespace sms.api.Managers.Shared
{
    public class AnnouncementManager : IAnnouncementManager
    {
        private readonly ApplicationDbContext _context;
        private readonly IFirebaseNotificationService? _firebaseService;

        public AnnouncementManager(ApplicationDbContext context, IFirebaseNotificationService? firebaseService = null)
        {
            _context = context;
            _firebaseService = firebaseService;
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            return await _context.Announcements
                .Include(a => a.Creator)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetPublishedAsync()
        {
            return await _context.Announcements
                .Include(a => a.Creator)
                .Where(a => a.IsPublished &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.UtcNow))
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetFeaturedAsync()
        {
            return await _context.Announcements
                .Include(a => a.Creator)
                .Where(a => a.IsPublished && a.IsFeatured &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.UtcNow))
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }

        public async Task<Announcement?> GetByIdAsync(Guid id)
        {
            return await _context.Announcements
                .Include(a => a.Creator)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Announcement> CreateAsync(Announcement announcement)
        {
            announcement.Id = Guid.NewGuid();
            announcement.CreatedAt = DateTime.UtcNow;

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            // Send Firebase notification if announcement is published
            if (announcement.IsPublished && _firebaseService != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _firebaseService.SendNotificationToAllUsersAsync(
                            announcement.Title ?? "New Announcement",
                            announcement.Content ?? "A new announcement has been published.",
                            new Dictionary<string, string>
                            {
                                { "type", "announcement" },
                                { "announcementId", announcement.Id.ToString() },
                                { "priority", announcement.Priority ?? "Normal" }
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail the announcement creation
                        Console.WriteLine($"Error sending notification: {ex.Message}");
                    }
                });
            }

            return announcement;
        }

        public async Task<Announcement> UpdateAsync(Announcement announcement)
        {
            var existing = await _context.Announcements.FindAsync(announcement.Id);
            var wasPublished = existing?.IsPublished ?? false;
            
            announcement.UpdatedAt = DateTime.UtcNow;
            _context.Announcements.Update(announcement);
            await _context.SaveChangesAsync();

            // Send notification if announcement is newly published
            if (!wasPublished && announcement.IsPublished && _firebaseService != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _firebaseService.SendNotificationToAllUsersAsync(
                            announcement.Title ?? "New Announcement",
                            announcement.Content ?? "A new announcement has been published.",
                            new Dictionary<string, string>
                            {
                                { "type", "announcement" },
                                { "announcementId", announcement.Id.ToString() },
                                { "priority", announcement.Priority ?? "Normal" }
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending notification: {ex.Message}");
                    }
                });
            }

            return announcement;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Announcement>> GetByCategoryAsync(string category)
        {
            return await _context.Announcements
                .Include(a => a.Creator)
                .Where(a => a.IsPublished && a.Category == category &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.UtcNow))
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetByPriorityAsync(string priority)
        {
            return await _context.Announcements
                .Include(a => a.Creator)
                .Where(a => a.IsPublished && a.Priority == priority &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.UtcNow))
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }
    }
}
