using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IOnlineClassManager
    {
        Task<IEnumerable<OnlineClass>> GetAllOnlineClassesAsync();
        Task<OnlineClass?> GetOnlineClassByIdAsync(Guid id);
        Task<IEnumerable<OnlineClass>> GetOnlineClassesByTeacherAsync(Guid teacherId);
        Task<IEnumerable<OnlineClass>> GetOnlineClassesByClassIdAsync(Guid classId);
        Task<IEnumerable<OnlineClass>> GetUpcomingOnlineClassesAsync();
        Task<IEnumerable<OnlineClass>> GetLiveOnlineClassesAsync();
        Task<OnlineClass> CreateOnlineClassAsync(OnlineClass onlineClass);
        Task<OnlineClass> UpdateOnlineClassAsync(OnlineClass onlineClass);
        Task<bool> DeleteOnlineClassAsync(Guid id);
        Task<bool> StartOnlineClassAsync(Guid id);
        Task<bool> EndOnlineClassAsync(Guid id);
        Task<bool> JoinOnlineClassAsync(Guid classId, Guid userId);
        Task<bool> LeaveOnlineClassAsync(Guid classId, Guid userId);
        Task<IEnumerable<OnlineClassParticipant>> GetClassParticipantsAsync(Guid classId);
        Task<IEnumerable<OnlineClassParticipant>> GetParticipantsAsync(Guid classId);
        Task<IEnumerable<ClassMessage>> GetClassMessagesAsync(Guid classId);
        Task<IEnumerable<ClassNote>> GetClassNotesAsync(Guid classId);
        Task<bool> CanUserJoinClassAsync(Guid classId, Guid userId);
        Task<ClassMessage> SendMessageAsync(ClassMessage message);
        Task<ClassNote> CreateNoteAsync(ClassNote note);
        Task<bool> UpdateParticipantStatusAsync(Guid classId, Guid userId, bool isMicEnabled, bool isCameraEnabled, bool isHandRaised);
    }
}
