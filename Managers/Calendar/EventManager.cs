using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using sms.api.Data;
using sms.api.Interfaces.Calendar;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Managers.Calendar
{
    public class EventManager : IEventManager
    {
        private readonly ApplicationDbContext _context;

        public EventManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EventDTO>> GetAllEventsAsync()
        {
            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return events.Select(MapToDTO);
        }

        public async Task<IEnumerable<EventDTO>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .Where(e => e.IsActive &&
                           ((e.StartDate >= startDate && e.StartDate <= endDate) ||
                            (e.EndDate >= startDate && e.EndDate <= endDate) ||
                            (e.StartDate <= startDate && e.EndDate >= endDate)))
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return events.Select(MapToDTO);
        }

        public async Task<IEnumerable<EventDTO>> GetEventsForUserAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var user = await _context.Users
                .Include(u => u.Student)
                .Include(u => u.Staff)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return new List<EventDTO>();

            var query = _context.Events
                .Include(e => e.CreatedBy)
                .Where(e => e.IsActive);

            // Filter by date range if provided
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(e =>
                    ((e.StartDate >= startDate && e.StartDate <= endDate) ||
                     (e.EndDate >= startDate && e.EndDate <= endDate) ||
                     (e.StartDate <= startDate && e.EndDate >= endDate)));
            }

            var events = await query.OrderBy(e => e.StartDate).ToListAsync();

            // Filter events based on user's classes
            var filteredEvents = new List<Event>();
           
            foreach (var eventItem in events)
            {
                if (eventItem.IsSchoolWide)
                {
                    filteredEvents.Add(eventItem);
                }
                else if (!string.IsNullOrEmpty(eventItem.ClassIds))
                {
                    var classIds = JsonSerializer.Deserialize<List<Guid>>(eventItem.ClassIds) ?? new List<Guid>();
                   
                    if (user.Student != null)
                    {
                        var studentClassIds = await _context.ClassEnrollments
                            .Where(ce => ce.StudentId == user.Student.Id)
                            .Select(ce => ce.ClassId)
                            .ToListAsync();
                       
                        if (classIds.Any(cId => studentClassIds.Contains(cId)))
                        {
                            filteredEvents.Add(eventItem);
                        }
                    }
                    else if (user.Staff != null)
                    {
                        var staffClassIds = await _context.Classes
                            .Where(c => c.TeacherId == user.Staff.Id)
                            .Select(c => c.Id)
                            .ToListAsync();
                       
                        if (classIds.Any(cId => staffClassIds.Contains(cId)))
                        {
                            filteredEvents.Add(eventItem);
                        }
                    }
                }
            }

            return filteredEvents.Select(MapToDTO);
        }

        public async Task<EventDTO?> GetEventByIdAsync(Guid id)
        {
            var eventItem = await _context.Events
                .Include(e => e.CreatedBy)
                .FirstOrDefaultAsync(e => e.Id == id);

            return eventItem != null ? MapToDTO(eventItem) : null;
        }

        public async Task<EventDTO> CreateEventAsync(CreateEventDTO createEventDto, Guid createdByUserId)
        {
            var eventItem = new Event
            {
                Id = Guid.NewGuid(),
                Title = createEventDto.Title,
                Description = createEventDto.Description,
                StartDate = createEventDto.StartDate,
                EndDate = createEventDto.EndDate,
                Location = createEventDto.Location,
                EventType = createEventDto.EventType,
                Priority = createEventDto.Priority,
                IsSchoolWide = createEventDto.IsSchoolWide,
                ClassIds = createEventDto.ClassIds != null && createEventDto.ClassIds.Any()
                    ? JsonSerializer.Serialize(createEventDto.ClassIds)
                    : null,
                IsActive = createEventDto.IsActive,
                RequiresAttendance = createEventDto.RequiresAttendance,
                AttachmentUrl = createEventDto.AttachmentUrl,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return await GetEventByIdAsync(eventItem.Id) ?? throw new InvalidOperationException("Failed to create event");
        }

        public async Task<EventDTO?> UpdateEventAsync(Guid id, UpdateEventDTO updateEventDto)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return null;

            if (!string.IsNullOrEmpty(updateEventDto.Title))
                eventItem.Title = updateEventDto.Title;
           
            if (!string.IsNullOrEmpty(updateEventDto.Description))
                eventItem.Description = updateEventDto.Description;
           
            if (updateEventDto.StartDate.HasValue)
                eventItem.StartDate = updateEventDto.StartDate.Value;
           
            if (updateEventDto.EndDate.HasValue)
                eventItem.EndDate = updateEventDto.EndDate.Value;
           
            if (!string.IsNullOrEmpty(updateEventDto.Location))
                eventItem.Location = updateEventDto.Location;
           
            if (!string.IsNullOrEmpty(updateEventDto.EventType))
                eventItem.EventType = updateEventDto.EventType;
           
            if (!string.IsNullOrEmpty(updateEventDto.Priority))
                eventItem.Priority = updateEventDto.Priority;
           
            if (updateEventDto.IsSchoolWide.HasValue)
                eventItem.IsSchoolWide = updateEventDto.IsSchoolWide.Value;
           
            if (updateEventDto.ClassIds != null)
                eventItem.ClassIds = updateEventDto.ClassIds.Any()
                    ? JsonSerializer.Serialize(updateEventDto.ClassIds)
                    : null;
           
            if (updateEventDto.IsActive.HasValue)
                eventItem.IsActive = updateEventDto.IsActive.Value;
           
            if (updateEventDto.RequiresAttendance.HasValue)
                eventItem.RequiresAttendance = updateEventDto.RequiresAttendance.Value;
           
            if (updateEventDto.AttachmentUrl != null)
                eventItem.AttachmentUrl = updateEventDto.AttachmentUrl;

            eventItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetEventByIdAsync(id);
        }

        public async Task<bool> DeleteEventAsync(Guid id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return false;

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EventExistsAsync(Guid id)
        {
            return await _context.Events.AnyAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<EventDTO>> GetActiveEventsAsync()
        {
            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .Where(e => e.IsActive)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return events.Select(MapToDTO);
        }

        public async Task<IEnumerable<EventDTO>> GetEventsByTypeAsync(string eventType)
        {
            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .Where(e => e.IsActive && e.EventType == eventType)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return events.Select(MapToDTO);
        }

        public async Task<IEnumerable<EventDTO>> GetEventsByPriorityAsync(string priority)
        {
            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .Where(e => e.IsActive && e.Priority == priority)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return events.Select(MapToDTO);
        }

        public async Task<IEnumerable<EventDTO>> GetUpcomingEventsAsync(int days = 7)
        {
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(days);

            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .Where(e => e.IsActive && e.StartDate >= startDate && e.StartDate <= endDate)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return events.Select(MapToDTO);
        }

        private static EventDTO MapToDTO(Event eventItem)
        {
            return new EventDTO
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                StartDate = eventItem.StartDate,
                EndDate = eventItem.EndDate,
                Location = eventItem.Location,
                EventType = eventItem.EventType,
                Priority = eventItem.Priority,
                IsSchoolWide = eventItem.IsSchoolWide,
                ClassIds = !string.IsNullOrEmpty(eventItem.ClassIds)
                    ? JsonSerializer.Deserialize<List<Guid>>(eventItem.ClassIds)
                    : null,
                IsActive = eventItem.IsActive,
                RequiresAttendance = eventItem.RequiresAttendance,
                AttachmentUrl = eventItem.AttachmentUrl,
                CreatedByName = eventItem.CreatedBy?.FullName ?? "Unknown",
                CreatedAt = eventItem.CreatedAt,
                UpdatedAt = eventItem.UpdatedAt
            };
        }
    }
}
