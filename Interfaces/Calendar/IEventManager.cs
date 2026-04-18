using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Calendar
{
    public interface IEventManager
    {
        Task<IEnumerable<EventDTO>> GetAllEventsAsync();
        Task<IEnumerable<EventDTO>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<EventDTO>> GetEventsForUserAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<EventDTO?> GetEventByIdAsync(Guid id);
        Task<EventDTO> CreateEventAsync(CreateEventDTO createEventDto, Guid createdByUserId);
        Task<EventDTO?> UpdateEventAsync(Guid id, UpdateEventDTO updateEventDto);
        Task<bool> DeleteEventAsync(Guid id);
        Task<bool> EventExistsAsync(Guid id);
        Task<IEnumerable<EventDTO>> GetActiveEventsAsync();
        Task<IEnumerable<EventDTO>> GetEventsByTypeAsync(string eventType);
        Task<IEnumerable<EventDTO>> GetEventsByPriorityAsync(string priority);
        Task<IEnumerable<EventDTO>> GetUpcomingEventsAsync(int days = 7);
    }
}
