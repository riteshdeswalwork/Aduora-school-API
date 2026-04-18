using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IAnnouncementManager
    {
        Task<IEnumerable<Announcement>> GetAllAsync();
        Task<IEnumerable<Announcement>> GetPublishedAsync();
        Task<IEnumerable<Announcement>> GetFeaturedAsync();
        Task<Announcement?> GetByIdAsync(Guid id);
        Task<Announcement> CreateAsync(Announcement announcement);
        Task<Announcement> UpdateAsync(Announcement announcement);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<Announcement>> GetByCategoryAsync(string category);
        Task<IEnumerable<Announcement>> GetByPriorityAsync(string priority);
    }
}
