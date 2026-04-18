using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class OnlineClassManager : IOnlineClassManager
    {
        private readonly ApplicationDbContext _context;

        public OnlineClassManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OnlineClass>> GetAllOnlineClassesAsync()
        {
            return await _context.OnlineClasses
                .Include(oc => oc.Class)
                .Include(oc => oc.Teacher)
                .ThenInclude(t => t.User)
                .OrderByDescending(oc => oc.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<OnlineClass?> GetOnlineClassByIdAsync(Guid id)
        {
            return await _context.OnlineClasses
                .Include(oc => oc.Class)
                .Include(oc => oc.Teacher)
                .ThenInclude(t => t.User)
                .Include(oc => oc.Participants)
                .ThenInclude(p => p.User)
                .Include(oc => oc.Messages)
                .ThenInclude(m => m.Sender)
                .Include(oc => oc.Notes)
                .ThenInclude(n => n.CreatedBy)
                .FirstOrDefaultAsync(oc => oc.Id == id);
        }

        public async Task<IEnumerable<OnlineClass>> GetOnlineClassesByTeacherAsync(Guid teacherId)
        {
            return await _context.OnlineClasses
                .Include(oc => oc.Class)
                .Include(oc => oc.Teacher)
                .ThenInclude(t => t.User)
                .Where(oc => oc.TeacherId == teacherId)
                .OrderByDescending(oc => oc.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<OnlineClass>> GetOnlineClassesByClassIdAsync(Guid classId)
        {
            return await _context.OnlineClasses
                .Include(oc => oc.Class)
                .Include(oc => oc.Teacher)
                .ThenInclude(t => t.User)
                .Where(oc => oc.ClassId == classId)
                .OrderByDescending(oc => oc.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<OnlineClass>> GetUpcomingOnlineClassesAsync()
        {
            return await _context.OnlineClasses
                .Include(oc => oc.Class)
                .Include(oc => oc.Teacher)
                .ThenInclude(t => t.User)
                .Where(oc => oc.Status == "Scheduled" && oc.ScheduledStartTime > DateTime.UtcNow)
                .OrderBy(oc => oc.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<OnlineClass>> GetLiveOnlineClassesAsync()
        {
            return await _context.OnlineClasses
                .Include(oc => oc.Class)
                .Include(oc => oc.Teacher)
                .ThenInclude(t => t.User)
                .Where(oc => oc.Status == "Live")
                .OrderBy(oc => oc.ActualStartTime)
                .ToListAsync();
        }

        public async Task<OnlineClass> CreateOnlineClassAsync(OnlineClass onlineClass)
        {
            onlineClass.Id = Guid.NewGuid();
            onlineClass.MeetingId = GenerateMeetingId();
            onlineClass.CreatedAt = DateTime.UtcNow;

            _context.OnlineClasses.Add(onlineClass);
            await _context.SaveChangesAsync();

            return onlineClass;
        }

        public async Task<OnlineClass> UpdateOnlineClassAsync(OnlineClass onlineClass)
        {
            onlineClass.UpdatedAt = DateTime.UtcNow;
            _context.OnlineClasses.Update(onlineClass);
            await _context.SaveChangesAsync();
            return onlineClass;
        }

        public async Task<bool> DeleteOnlineClassAsync(Guid id)
        {
            var onlineClass = await _context.OnlineClasses.FindAsync(id);
            if (onlineClass == null)
                return false;

            _context.OnlineClasses.Remove(onlineClass);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> StartOnlineClassAsync(Guid id)
        {
            var onlineClass = await _context.OnlineClasses.FindAsync(id);
            if (onlineClass == null)
                return false;

            onlineClass.Status = "Live";
            onlineClass.ActualStartTime = DateTime.UtcNow;
            onlineClass.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EndOnlineClassAsync(Guid id)
        {
            var onlineClass = await _context.OnlineClasses.FindAsync(id);
            if (onlineClass == null)
                return false;

            onlineClass.Status = "Ended";
            onlineClass.ActualEndTime = DateTime.UtcNow;
            onlineClass.UpdatedAt = DateTime.UtcNow;

            // Mark all participants as left
            var participants = await _context.OnlineClassParticipants
                .Where(p => p.OnlineClassId == id && p.IsPresent)
                .ToListAsync();

            foreach (var participant in participants)
            {
                participant.LeftAt = DateTime.UtcNow;
                participant.IsPresent = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> JoinOnlineClassAsync(Guid classId, Guid userId)
        {
            // Check if user can join this class
            if (!await CanUserJoinClassAsync(classId, userId))
                return false;

            var onlineClass = await _context.OnlineClasses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == classId);
            if (onlineClass == null)
                return false;

            // Check if participant already exists
            var existingParticipant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == classId && p.UserId == userId);

            if (existingParticipant != null)
            {
                // Update existing participant
                existingParticipant.JoinedAt = DateTime.UtcNow;
                existingParticipant.IsPresent = true;
                existingParticipant.LastActivity = DateTime.UtcNow;
            }
            else
            {
                // Create new participant
                var participant = new OnlineClassParticipant
                {
                    Id = Guid.NewGuid(),
                    OnlineClassId = classId,
                    AcademicYearId = onlineClass.AcademicYearId,
                    UserId = userId,
                    Role = await GetUserRoleInClassAsync(classId, userId),
                    JoinedAt = DateTime.UtcNow,
                    IsPresent = true,
                    LastActivity = DateTime.UtcNow
                };

                _context.OnlineClassParticipants.Add(participant);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LeaveOnlineClassAsync(Guid classId, Guid userId)
        {
            var participant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == classId && p.UserId == userId);

            if (participant == null)
                return false;

            participant.LeftAt = DateTime.UtcNow;
            participant.IsPresent = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<OnlineClassParticipant>> GetClassParticipantsAsync(Guid classId)
        {
            return await _context.OnlineClassParticipants
                .Include(p => p.User)
                .Where(p => p.OnlineClassId == classId)
                .OrderBy(p => p.JoinedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassMessage>> GetClassMessagesAsync(Guid classId)
        {
            return await _context.ClassMessages
                .Include(m => m.Sender)
                .Where(m => m.OnlineClassId == classId && !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassNote>> GetClassNotesAsync(Guid classId)
        {
            return await _context.ClassNotes
                .Include(n => n.CreatedBy)
                .Where(n => n.OnlineClassId == classId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CanUserJoinClassAsync(Guid classId, Guid userId)
        {
            var onlineClass = await _context.OnlineClasses
                .Include(oc => oc.Class)
                .FirstOrDefaultAsync(oc => oc.Id == classId);

            if (onlineClass == null)
                return false;

            // Enforce participant capacity (for very large classes this is still cheap with index and COUNT)
            var currentParticipants = await _context.OnlineClassParticipants
                .CountAsync(p => p.OnlineClassId == classId && p.IsPresent);
            if (onlineClass.MaxParticipants > 0 && currentParticipants >= onlineClass.MaxParticipants)
            {
                return false;
            }

            // Check if user is the teacher
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.UserId == userId);
            if (staff != null && staff.Id == onlineClass.TeacherId)
                return true;

            // Allow staff/admin users to join for supervision/support
            if (staff != null)
                return true;

            // Check if user is a student in this class
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student != null)
            {
                var enrollment = await _context.ClassEnrollments
                    .FirstOrDefaultAsync(ce => ce.StudentId == student.Id && ce.ClassId == onlineClass.ClassId);
                return enrollment != null;
            }

            // Fallback for authenticated users who are not in student/staff table yet.
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<ClassMessage> SendMessageAsync(ClassMessage message)
        {
            message.Id = Guid.NewGuid();
            message.SentAt = DateTime.UtcNow;

            _context.ClassMessages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<ClassNote> CreateNoteAsync(ClassNote note)
        {
            note.Id = Guid.NewGuid();
            note.CreatedAt = DateTime.UtcNow;

            _context.ClassNotes.Add(note);
            await _context.SaveChangesAsync();

            // Load the note with user information
            return await _context.ClassNotes
                .Include(n => n.CreatedBy)
                .FirstOrDefaultAsync(n => n.Id == note.Id) ?? note;
        }

        public async Task<IEnumerable<OnlineClassParticipant>> GetParticipantsAsync(Guid classId)
        {
            return await _context.OnlineClassParticipants
                .Include(p => p.User)
                .Where(p => p.OnlineClassId == classId && p.IsPresent)
                .OrderBy(p => p.Role == "Teacher" ? 0 : 1)
                .ThenBy(p => p.User.FirstName)
                .ToListAsync();
        }



        public async Task<bool> UpdateParticipantStatusAsync(Guid classId, Guid userId, bool isMicEnabled, bool isCameraEnabled, bool isHandRaised)
        {
            var participant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == classId && p.UserId == userId);

            if (participant == null)
                return false;

            participant.IsMicEnabled = isMicEnabled;
            participant.IsCameraEnabled = isCameraEnabled;
            participant.IsHandRaised = isHandRaised;
            participant.LastActivity = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateMeetingId()
        {
            return Guid.NewGuid().ToString("N")[..10].ToUpper();
        }

        private async Task<string> GetUserRoleInClassAsync(Guid classId, Guid userId)
        {
            var onlineClass = await _context.OnlineClasses.FindAsync(classId);
            if (onlineClass == null)
                return "Student";

            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.UserId == userId);
            if (staff != null && staff.Id == onlineClass.TeacherId)
                return "Teacher";

            return "Student";
        }
    }
}
