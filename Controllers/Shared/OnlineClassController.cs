using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Interfaces;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using System.Security.Claims;

namespace sms.api.Controllers.Shared
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OnlineClassController : ApiControllerBase
    {
        private readonly IOnlineClassManager _onlineClassManager;
        private readonly IStaffManager _staffManager;
        private readonly IClassManager _classManager;
        private readonly IAcademicYearManager _academicYearManager;
        private readonly ILiveKitTokenService _liveKitTokenService;
        private readonly IConfiguration _configuration;

        public OnlineClassController(
            IOnlineClassManager onlineClassManager,
            IStaffManager staffManager,
            IClassManager classManager,
            IAcademicYearManager academicYearManager,
            ILiveKitTokenService liveKitTokenService,
            IConfiguration configuration)
        {
            _onlineClassManager = onlineClassManager;
            _staffManager = staffManager;
            _classManager = classManager;
            _academicYearManager = academicYearManager;
            _liveKitTokenService = liveKitTokenService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OnlineClass>>> GetAllOnlineClasses()
        {
            try
            {
                var onlineClasses = await _onlineClassManager.GetAllOnlineClassesAsync();
                return Ok(onlineClasses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OnlineClass>> GetOnlineClassById(Guid id)
        {
            try
            {
                var onlineClass = await _onlineClassManager.GetOnlineClassByIdAsync(id);
                if (onlineClass == null)
                    return NotFound(new { error = "Online class not found" });

                return Ok(onlineClass);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<ActionResult<IEnumerable<OnlineClass>>> GetOnlineClassesByTeacher(Guid teacherId)
        {
            try
            {
                var onlineClasses = await _onlineClassManager.GetOnlineClassesByTeacherAsync(teacherId);
                return Ok(onlineClasses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("class/{classId}")]
        public async Task<ActionResult<IEnumerable<OnlineClass>>> GetOnlineClassesByClassId(Guid classId)
        {
            try
            {
                var onlineClasses = await _onlineClassManager.GetOnlineClassesByClassIdAsync(classId);
                return Ok(onlineClasses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<OnlineClass>>> GetUpcomingOnlineClasses()
        {
            try
            {
                var onlineClasses = await _onlineClassManager.GetUpcomingOnlineClassesAsync();
                return Ok(onlineClasses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("live")]
        public async Task<ActionResult<IEnumerable<OnlineClass>>> GetLiveOnlineClasses()
        {
            try
            {
                var onlineClasses = await _onlineClassManager.GetLiveOnlineClassesAsync();
                return Ok(onlineClasses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<OnlineClass>> CreateOnlineClass([FromBody] CreateOnlineClassRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = GetCurrentUserId();
                var teacherId = request.TeacherId;
                var selectedClass = await _classManager.GetClassByIdAsync(request.ClassId);
                if (selectedClass == null)
                {
                    return BadRequest(new { error = "Selected class does not exist." });
                }

                if (teacherId == null || teacherId == Guid.Empty)
                {
                    var currentStaff = await _staffManager.GetByUserIdAsync(currentUserId);
                    if (currentStaff != null)
                    {
                        teacherId = currentStaff.Id;
                    }
                    else
                    {
                        // Admin can create classes; auto-pick teacher from selected class or first available staff.
                        if (selectedClass?.TeacherId != null && selectedClass.TeacherId != Guid.Empty)
                        {
                            teacherId = selectedClass.TeacherId;
                        }
                        else
                        {
                            var fallbackTeacher = (await _staffManager.GetAllAsync()).FirstOrDefault();
                            if (fallbackTeacher != null)
                            {
                                teacherId = fallbackTeacher.Id;
                            }
                        }

                        if (teacherId == null || teacherId == Guid.Empty)
                        {
                            return BadRequest(new { error = "No staff found. Please add staff or provide TeacherId." });
                        }
                    }
                }

                var academicYearId = GetCurrentAcademicYearId();
                if (academicYearId != null && academicYearId != Guid.Empty)
                {
                    var yearFromHeader = await _academicYearManager.GetAcademicYearByIdAsync(academicYearId.Value);
                    if (yearFromHeader == null)
                    {
                        academicYearId = null;
                    }
                }

                if (academicYearId == null || academicYearId == Guid.Empty)
                {
                    var currentAcademicYear = await _academicYearManager.GetCurrentAcademicYearAsync();
                    if (currentAcademicYear == null)
                    {
                        return BadRequest(new { error = "No active academic year found. Please set current academic year first." });
                    }

                    academicYearId = currentAcademicYear.Id;
                }

                var teacher = await _staffManager.GetByIdAsync(teacherId.Value);
                if (teacher == null)
                {
                    return BadRequest(new { error = "Resolved teacher does not exist. Please select a valid teacher." });
                }

                var onlineClass = new OnlineClass
                {
                    Title = request.Title,
                    Description = request.Description ?? string.Empty,
                    ClassId = request.ClassId,
                    TeacherId = teacherId.Value,
                    AcademicYearId = academicYearId.Value,
                    ScheduledStartTime = request.ScheduledStartTime,
                    ScheduledEndTime = request.ScheduledEndTime,
                    IsRecordingEnabled = request.IsRecordingEnabled,
                    IsChatEnabled = request.IsChatEnabled,
                    IsNotesEnabled = request.IsNotesEnabled,
                    MaxParticipants = Math.Clamp(request.MaxParticipants, 1, 10000)
                };

                var createdClass = await _onlineClassManager.CreateOnlineClassAsync(onlineClass);
                return CreatedAtAction(nameof(GetOnlineClassById), new { id = createdClass.Id }, createdClass);
            }
            catch (DbUpdateException ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = $"Database save failed: {detail}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<OnlineClass>> UpdateOnlineClass(Guid id, [FromBody] UpdateOnlineClassRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingClass = await _onlineClassManager.GetOnlineClassByIdAsync(id);
                if (existingClass == null)
                    return NotFound(new { error = "Online class not found" });

                existingClass.Title = request.Title;
                existingClass.Description = request.Description;
                existingClass.ScheduledStartTime = request.ScheduledStartTime;
                existingClass.ScheduledEndTime = request.ScheduledEndTime;
                existingClass.IsRecordingEnabled = request.IsRecordingEnabled;
                existingClass.IsChatEnabled = request.IsChatEnabled;
                existingClass.IsNotesEnabled = request.IsNotesEnabled;
                existingClass.MaxParticipants = Math.Clamp(request.MaxParticipants, 1, 10000);

                var updatedClass = await _onlineClassManager.UpdateOnlineClassAsync(existingClass);
                return Ok(updatedClass);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOnlineClass(Guid id)
        {
            try
            {
                var result = await _onlineClassManager.DeleteOnlineClassAsync(id);
                if (!result)
                    return NotFound(new { error = "Online class not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("{id}/start")]
        public async Task<ActionResult> StartOnlineClass(Guid id)
        {
            try
            {
                var result = await _onlineClassManager.StartOnlineClassAsync(id);
                if (!result)
                    return NotFound(new { error = "Online class not found" });

                return Ok(new { message = "Online class started successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("{id}/end")]
        public async Task<ActionResult> EndOnlineClass(Guid id)
        {
            try
            {
                var result = await _onlineClassManager.EndOnlineClassAsync(id);
                if (!result)
                    return NotFound(new { error = "Online class not found" });

                return Ok(new { message = "Online class ended successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("{id}/join")]
        public async Task<ActionResult> JoinOnlineClass(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                var result = await _onlineClassManager.JoinOnlineClassAsync(id, userId);
                if (!result)
                    return BadRequest(new { error = "Unable to join online class" });

                return Ok(new { message = "Joined online class successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("{id}/leave")]
        public async Task<ActionResult> LeaveOnlineClass(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                var result = await _onlineClassManager.LeaveOnlineClassAsync(id, userId);
                if (!result)
                    return BadRequest(new { error = "Unable to leave online class" });

                return Ok(new { message = "Left online class successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }



        [HttpGet("{id}/can-join")]
        public async Task<ActionResult<bool>> CanUserJoinClass(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                var canJoin = await _onlineClassManager.CanUserJoinClassAsync(id, userId);
                return Ok(new { canJoin });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}/participants")]
        public async Task<ActionResult<IEnumerable<OnlineClassParticipant>>> GetParticipants(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                // Check if user can access this class
                var canJoin = await _onlineClassManager.CanUserJoinClassAsync(id, userId);
                if (!canJoin)
                    return Forbid();

                var participants = await _onlineClassManager.GetParticipantsAsync(id);
                return Ok(participants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}/messages")]
        public async Task<ActionResult<IEnumerable<ClassMessage>>> GetMessages(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                // Check if user can access this class
                var canJoin = await _onlineClassManager.CanUserJoinClassAsync(id, userId);
                if (!canJoin)
                    return Forbid();

                var messages = await _onlineClassManager.GetClassMessagesAsync(id);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}/livekit-token")]
        public async Task<ActionResult<LiveKitAccessResponse>> GetLiveKitToken(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                var onlineClass = await _onlineClassManager.GetOnlineClassByIdAsync(id);
                if (onlineClass == null)
                    return NotFound(new { error = "Online class not found." });

                var canJoin = await _onlineClassManager.CanUserJoinClassAsync(id, userId);
                if (!canJoin)
                    return Forbid();

                var roomName = !string.IsNullOrWhiteSpace(onlineClass.MeetingId)
                    ? onlineClass.MeetingId
                    : $"class-{onlineClass.Id:N}";

                var displayName = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = $"User-{userId.ToString()[..8]}";
                }

                var token = _liveKitTokenService.BuildParticipantToken(
                    identity: userId.ToString(),
                    participantName: displayName,
                    roomName: roomName);

                var serverUrl = _configuration["LiveKit:Url"] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(serverUrl))
                {
                    return StatusCode(500, new { error = "LiveKit URL is not configured." });
                }

                return Ok(new LiveKitAccessResponse
                {
                    Token = token,
                    Url = serverUrl,
                    RoomName = roomName,
                    Identity = userId.ToString(),
                    Name = displayName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}/notes")]
        public async Task<ActionResult<IEnumerable<ClassNote>>> GetNotes(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                // Check if user can access this class
                var canJoin = await _onlineClassManager.CanUserJoinClassAsync(id, userId);
                if (!canJoin)
                    return Forbid();

                var notes = await _onlineClassManager.GetClassNotesAsync(id);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("notes")]
        public async Task<ActionResult<ClassNote>> CreateNote([FromBody] CreateNoteRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                // Check if user can access this class
                var canJoin = await _onlineClassManager.CanUserJoinClassAsync(request.OnlineClassId, userId);
                if (!canJoin)
                    return Forbid();

                var note = new ClassNote
                {
                    OnlineClassId = request.OnlineClassId,
                    CreatedByUserId = userId,
                    Title = request.Title,
                    Content = request.Content,
                    NoteType = request.NoteType ?? "Text",
                    IsPublic = request.IsPublic,
                    IsPinned = request.IsPinned
                };

                var createdNote = await _onlineClassManager.CreateNoteAsync(note);
                return Ok(createdNote);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }

    public class CreateOnlineClassRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ClassId { get; set; }
        public Guid? TeacherId { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public bool IsRecordingEnabled { get; set; } = false;
        public bool IsChatEnabled { get; set; } = true;
        public bool IsNotesEnabled { get; set; } = true;
        public int MaxParticipants { get; set; } = 10000;
    }

    public class UpdateOnlineClassRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public bool IsRecordingEnabled { get; set; } = false;
        public bool IsChatEnabled { get; set; } = true;
        public bool IsNotesEnabled { get; set; } = true;
        public int MaxParticipants { get; set; } = 10000;
    }

    public class CreateNoteRequest
    {
        public Guid OnlineClassId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? NoteType { get; set; } = "Text";
        public bool IsPublic { get; set; } = true;
        public bool IsPinned { get; set; } = false;
    }

    public class LiveKitAccessResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Identity { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
