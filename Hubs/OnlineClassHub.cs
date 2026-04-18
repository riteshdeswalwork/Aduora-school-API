using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Models;
using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace sms.api.Hubs
{
    [Authorize]
    public class OnlineClassHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _screenSharerByClass = new();
        private readonly ApplicationDbContext _context;

        public OnlineClassHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var userName = Context.User?.Identity?.Name;
            var isAuthenticated = Context.User?.Identity?.IsAuthenticated ?? false;

            Console.WriteLine($"DEBUG HUB: OnConnectedAsync - ConnectionId: {Context.ConnectionId}");
            Console.WriteLine($"DEBUG HUB: OnConnectedAsync - UserId: {userId}");
            Console.WriteLine($"DEBUG HUB: OnConnectedAsync - UserName: {userName}");
            Console.WriteLine($"DEBUG HUB: OnConnectedAsync - IsAuthenticated: {isAuthenticated}");

            // Log all claims for debugging
            if (Context.User?.Claims != null && Context.User.Claims.Any())
            {
                Console.WriteLine("DEBUG HUB: OnConnectedAsync - Claims:");
                foreach (var claim in Context.User.Claims)
                {
                    Console.WriteLine($"DEBUG HUB: Claim - Type: {claim.Type}, Value: {claim.Value}");
                }
            }
            else
            {
                Console.WriteLine("DEBUG HUB: OnConnectedAsync - No claims found or user not authenticated");
            }

            await base.OnConnectedAsync();
        }

        public async Task JoinClass(string classId)
        {
            var userId = GetCurrentUserId();
            var userName = Context.User?.Identity?.Name;
            Console.WriteLine($"DEBUG HUB: JoinClass called - UserId: {userId}, ClassId: {classId}, ConnectionId: {Context.ConnectionId}, UserName: {userName}");
            Console.WriteLine($"DEBUG HUB: User.Identity.IsAuthenticated: {Context.User?.Identity?.IsAuthenticated}");

            // Log all claims for debugging
            if (Context.User?.Claims != null)
            {
                foreach (var claim in Context.User.Claims)
                {
                    Console.WriteLine($"DEBUG HUB: Claim - Type: {claim.Type}, Value: {claim.Value}");
                }
            }
            else
            {
                Console.WriteLine("DEBUG HUB: No claims found");
            }

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("DEBUG HUB: ERROR - UserId is null or empty in JoinClass, returning");
                return;
            }

            if (!Guid.TryParse(classId, out var classGuid) || !Guid.TryParse(userId, out var userGuid))
            {
                return;
            }

            var onlineClass = await _context.OnlineClasses.FirstOrDefaultAsync(c => c.Id == classGuid);
            if (onlineClass == null || onlineClass.Status != "Live")
            {
                await Clients.Caller.SendAsync("JoinRejected", "Class is not live or does not exist.");
                return;
            }

            var currentParticipant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == classGuid && p.UserId == userGuid);

            if (currentParticipant == null || !currentParticipant.IsPresent)
            {
                var presentCount = await _context.OnlineClassParticipants
                    .CountAsync(p => p.OnlineClassId == classGuid && p.IsPresent);
                if (onlineClass.MaxParticipants > 0 && presentCount >= onlineClass.MaxParticipants)
                {
                    await Clients.Caller.SendAsync("JoinRejected", "Class has reached maximum participant limit.");
                    return;
                }
            }

            // Add user to the class group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"class_{classId}");
            Console.WriteLine($"DEBUG HUB: Added user to group: class_{classId}");

            // Update participant record
            var participant = currentParticipant;

            if (participant != null)
            {
                participant.ConnectionId = Context.ConnectionId;
                participant.LastActivity = DateTime.UtcNow;
                participant.IsPresent = true;
                await _context.SaveChangesAsync();
                Console.WriteLine("DEBUG HUB: Updated participant record");
            }
            else
            {
                var onlineClassEntity = await _context.OnlineClasses
                    .FirstOrDefaultAsync(c => c.Id == classGuid);
                if (onlineClassEntity != null)
                {
                    var participantRole = "Student";
                    var isTeacher = await _context.Staff.AnyAsync(s => s.UserId == userGuid && s.Id == onlineClassEntity.TeacherId);
                    if (isTeacher)
                    {
                        participantRole = "Teacher";
                    }

                    _context.OnlineClassParticipants.Add(new OnlineClassParticipant
                    {
                        Id = Guid.NewGuid(),
                        OnlineClassId = classGuid,
                        AcademicYearId = onlineClassEntity.AcademicYearId,
                        UserId = userGuid,
                        Role = participantRole,
                        ConnectionId = Context.ConnectionId,
                        JoinedAt = DateTime.UtcNow,
                        LastActivity = DateTime.UtcNow,
                        IsPresent = true,
                        IsMicEnabled = false,
                        IsCameraEnabled = false
                    });
                    await _context.SaveChangesAsync();
                }
            }

            // Get user info for notification
            var user = await _context.Users.FindAsync(userGuid);
            var userFullName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User";

            // Notify others that user joined
            await Clients.Group($"class_{classId}").SendAsync("UserJoined", userId, Context.ConnectionId);
            await Clients.Group($"class_{classId}").SendAsync("ParticipantJoined", userId, userFullName);
            Console.WriteLine("DEBUG HUB: Sent UserJoined and ParticipantJoined notifications");

            // Send updated participant count
            await BroadcastParticipantCount(classId);
        }

        public async Task LeaveClass(string classId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            if (!Guid.TryParse(classId, out var classGuid) || !Guid.TryParse(userId, out var userGuid))
            {
                return;
            }

            // Remove user from the class group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"class_{classId}");

            // Update participant record
            var participant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == classGuid && p.UserId == userGuid);

            if (participant != null)
            {
                participant.LeftAt = DateTime.UtcNow;
                participant.IsPresent = false;
                await _context.SaveChangesAsync();
            }

            // Notify others that user left
            await Clients.Group($"class_{classId}").SendAsync("UserLeft", userId);

            if (_screenSharerByClass.TryGetValue(classId, out var currentSharer) && currentSharer == userId)
            {
                _screenSharerByClass.TryRemove(classId, out _);
                await Clients.Group($"class_{classId}").SendAsync("ScreenShareStopped", userId);
            }

            // Send updated participant count
            await BroadcastParticipantCount(classId);
        }



        public async Task UpdateParticipantStatus(string classId, bool micEnabled, bool cameraEnabled, bool handRaised)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            if (!Guid.TryParse(classId, out var classGuid) || !Guid.TryParse(userId, out var userGuid))
            {
                return;
            }

            // Update participant record
            var participant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == classGuid && p.UserId == userGuid);

            if (participant != null)
            {
                participant.IsMicEnabled = micEnabled;
                participant.IsCameraEnabled = cameraEnabled;
                participant.IsHandRaised = handRaised;
                participant.LastActivity = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Notify others about the status change
                await Clients.Group($"class_{classId}").SendAsync("ParticipantUpdated", userId, micEnabled, cameraEnabled, handRaised);
            }
        }

        public async Task SendMessage(string classId, string message, string messageType = "Text")
        {
            var userId = GetCurrentUserId();
            Console.WriteLine($"DEBUG HUB: SendMessage called - UserId: {userId}, ClassId: {classId}, Message: {message}");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("DEBUG HUB: UserId is null or empty, returning");
                return;
            }

            if (!Guid.TryParse(classId, out var classGuid) || !Guid.TryParse(userId, out var userGuid))
            {
                return;
            }

            // Save message to database
            var classMessage = new ClassMessage
            {
                Id = Guid.NewGuid(),
                OnlineClassId = classGuid,
                SenderId = userGuid,
                Message = message,
                MessageType = messageType,
                SentAt = DateTime.UtcNow
            };

            _context.ClassMessages.Add(classMessage);
            await _context.SaveChangesAsync();
            Console.WriteLine("DEBUG HUB: Message saved to database");

            // Get sender info
            var sender = await _context.Users.FindAsync(userGuid);
            var senderName = $"{sender?.FirstName} {sender?.LastName}";
            Console.WriteLine($"DEBUG HUB: Sender info - Name: {senderName}");

            // Send message to all participants
            Console.WriteLine($"DEBUG HUB: Sending to group: class_{classId}");
            await Clients.Group($"class_{classId}").SendAsync("ReceiveMessage", userId, senderName, message, messageType, classMessage.SentAt);
            Console.WriteLine("DEBUG HUB: Message sent to group");
        }

        public async Task ToggleMic(string classId, bool isEnabled)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            // Update participant mic status
            var participant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == Guid.Parse(classId) && p.UserId == Guid.Parse(userId));

            if (participant != null)
            {
                participant.IsMicEnabled = isEnabled;
                await _context.SaveChangesAsync();
            }

            // Notify others about mic status change
            await Clients.Group($"class_{classId}").SendAsync("MicStatusChanged", userId, isEnabled);
        }

        public async Task ToggleCamera(string classId, bool isEnabled)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            // Update participant camera status
            var participant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == Guid.Parse(classId) && p.UserId == Guid.Parse(userId));

            if (participant != null)
            {
                participant.IsCameraEnabled = isEnabled;
                await _context.SaveChangesAsync();
            }

            // Notify others about camera status change
            await Clients.Group($"class_{classId}").SendAsync("CameraStatusChanged", userId, isEnabled);
        }

        public async Task RaiseHand(string classId, bool isRaised)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            // Update participant hand status
            var participant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == Guid.Parse(classId) && p.UserId == Guid.Parse(userId));

            if (participant != null)
            {
                participant.IsHandRaised = isRaised;
                await _context.SaveChangesAsync();
            }

            // Notify others about hand status change
            await Clients.Group($"class_{classId}").SendAsync("HandStatusChanged", userId, isRaised);
        }

        public async Task ShareNote(string classId, string title, string content, string noteType = "Text")
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            // Save note to database
            var note = new ClassNote
            {
                Id = Guid.NewGuid(),
                OnlineClassId = Guid.Parse(classId),
                CreatedByUserId = Guid.Parse(userId),
                Title = title,
                Content = content,
                NoteType = noteType,
                CreatedAt = DateTime.UtcNow
            };

            _context.ClassNotes.Add(note);
            await _context.SaveChangesAsync();

            // Get creator info
            var creator = await _context.Users.FindAsync(Guid.Parse(userId));

            // Send note to all participants
            await Clients.Group($"class_{classId}").SendAsync("ReceiveNote", new
            {
                Id = note.Id,
                Title = title,
                Content = content,
                NoteType = noteType,
                CreatedBy = $"{creator?.FirstName} {creator?.LastName}",
                CreatedAt = note.CreatedAt
            });
        }

        public async Task SendWebRTCSignal(string classId, string targetUserId, object signal)
        {
            // Forward WebRTC signaling data to specific user
            await Clients.User(targetUserId).SendAsync("ReceiveWebRTCSignal", GetCurrentUserId(), signal);
        }

        // WebRTC Signaling Methods
        public async Task SendOffer(string targetUserId, string offer)
        {
            var fromUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(fromUserId) || string.IsNullOrEmpty(targetUserId))
                return;

            await Clients.User(targetUserId).SendAsync("ReceiveOffer", fromUserId, offer);
            Console.WriteLine($"DEBUG HUB: Offer sent from {fromUserId} to {targetUserId}");
        }

        public async Task SendAnswer(string targetUserId, string answer)
        {
            var fromUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(fromUserId) || string.IsNullOrEmpty(targetUserId))
                return;

            await Clients.User(targetUserId).SendAsync("ReceiveAnswer", fromUserId, answer);
            Console.WriteLine($"DEBUG HUB: Answer sent from {fromUserId} to {targetUserId}");
        }

        public async Task SendIceCandidate(string targetUserId, string candidate)
        {
            var fromUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(fromUserId) || string.IsNullOrEmpty(targetUserId))
                return;

            await Clients.User(targetUserId).SendAsync("ReceiveIceCandidate", fromUserId, candidate);
            Console.WriteLine($"DEBUG HUB: ICE candidate sent from {fromUserId} to {targetUserId}");
        }

        public async Task StartScreenShare(string classId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            var groupKey = $"class_{classId}";
            var previousSharer = _screenSharerByClass.AddOrUpdate(classId, userId, (_, _) => userId);
            if (!string.IsNullOrEmpty(previousSharer) && previousSharer != userId)
            {
                await Clients.User(previousSharer).SendAsync("ScreenShareTakenOver");
            }

            await Clients.Group(groupKey).SendAsync("ScreenShareStarted", userId);
        }

        public async Task StopScreenShare(string classId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            _screenSharerByClass.TryRemove(classId, out _);
            await Clients.Group($"class_{classId}").SendAsync("ScreenShareStopped", userId);
        }

        public async Task RemoveParticipant(string classId, string targetUserId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            // Check if user is staff/teacher
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user?.Role != "Staff" && user?.Role != "Admin")
                return;

            // Remove participant from class
            var participant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == Guid.Parse(classId) && p.UserId == Guid.Parse(targetUserId));

            if (participant != null)
            {
                participant.IsPresent = false;
                participant.LeftAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Notify the participant to leave
            await Clients.User(targetUserId).SendAsync("ForceLeave", "You have been removed from the class by the teacher");

            // Notify others about the removal
            await Clients.Group($"class_{classId}").SendAsync("ParticipantRemoved", targetUserId);
        }

        public async Task MuteParticipant(string classId, string targetUserId, bool mute)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            // Check if user is staff/teacher
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user?.Role != "Staff" && user?.Role != "Admin")
                return;

            // Update participant mic status
            var participant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == Guid.Parse(classId) && p.UserId == Guid.Parse(targetUserId));

            if (participant != null)
            {
                participant.IsMicEnabled = !mute;
                await _context.SaveChangesAsync();
            }

            // Notify the participant about mic control
            await Clients.User(targetUserId).SendAsync("MicControlled", mute);

            // Notify others about the change
            await Clients.Group($"class_{classId}").SendAsync("ParticipantMicChanged", targetUserId, !mute);
        }

        public async Task ControlParticipantCamera(string classId, string targetUserId, bool enable)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            // Check if user is staff/teacher
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user?.Role != "Staff" && user?.Role != "Admin")
                return;

            // Update participant camera status
            var participant = await _context.OnlineClassParticipants
                .FirstOrDefaultAsync(p => p.OnlineClassId == Guid.Parse(classId) && p.UserId == Guid.Parse(targetUserId));

            if (participant != null)
            {
                participant.IsCameraEnabled = enable;
                await _context.SaveChangesAsync();
            }

            // Notify the participant about camera control
            await Clients.User(targetUserId).SendAsync("CameraControlled", enable);

            // Notify others about the change
            await Clients.Group($"class_{classId}").SendAsync("ParticipantCameraChanged", targetUserId, enable);
        }

        public async Task EndMeeting(string classId)
        {
            Console.WriteLine($"DEBUG HUB: EndMeeting called - ClassId: {classId}");

            try
            {
                // Notify all participants that meeting has ended
                await Clients.Group($"class_{classId}").SendAsync("MeetingEnded");

                Console.WriteLine($"DEBUG HUB: Meeting {classId} ended successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG HUB: Error ending meeting: {ex.Message}");
            }
        }

        private async Task BroadcastParticipantCount(string classId)
        {
            try
            {
                var participantCount = await _context.OnlineClassParticipants
                    .CountAsync(p => p.OnlineClassId == Guid.Parse(classId) && p.IsPresent);

                await Clients.Group($"class_{classId}").SendAsync("ParticipantCountUpdated", participantCount);
                Console.WriteLine($"DEBUG HUB: Broadcasted participant count: {participantCount} for class {classId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG HUB: Error broadcasting participant count: {ex.Message}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Handle user disconnection
            var userId = GetCurrentUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                // Update all participant records for this user
                var participants = await _context.OnlineClassParticipants
                    .Where(p => p.UserId == Guid.Parse(userId) && p.ConnectionId == Context.ConnectionId)
                    .ToListAsync();

                foreach (var participant in participants)
                {
                    participant.LeftAt = DateTime.UtcNow;
                    participant.IsPresent = false;
                }

                await _context.SaveChangesAsync();

                // Notify all groups this user was part of
                foreach (var participant in participants)
                {
                    await Clients.Group($"class_{participant.OnlineClassId}").SendAsync("UserLeft", userId);
                    await BroadcastParticipantCount(participant.OnlineClassId.ToString());
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        private string? GetCurrentUserId()
        {
            return Context.UserIdentifier
                ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("sub")?.Value
                ?? Context.User?.FindFirst("id")?.Value;
        }
    }
}
