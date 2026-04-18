using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sms.api.Controllers.Shared;
using sms.api.Data;
using sms.api.Models;

namespace sms.api.Controllers.Chat
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        private static readonly Dictionary<string, string[]> RoleChatMatrix = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Admin",       new[] { "Admin", "Staff", "Student", "Accountant", "Librarian" } },
            { "Staff",       new[] { "Admin", "Staff", "Student", "Librarian" } },
            { "Librarian",   new[] { "Admin", "Librarian", "Staff", "Student" } },
            { "Accountant",  new[] { "Admin", "Staff" } },
            { "Student",     new[] { "Admin", "Staff", "Librarian" } },
        };

        [HttpGet("partners")]
        public async Task<IActionResult> GetChatPartners()
        {
            var currentRole = GetCurrentUserRole();
            if (!RoleChatMatrix.TryGetValue(currentRole, out var allowedRoles))
                allowedRoles = Array.Empty<string>();

            var partners = await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && allowedRoles.Contains(u.Role))
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FirstName)
                .Select(u => new
                {
                    id = u.Id,
                    name = !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : $"{(u.FirstName ?? "").Trim()} {(u.LastName ?? "").Trim()}".Trim(),
                    email = u.Email,
                    role = u.Role,
                    online = false
                })
                .ToListAsync();

            return Ok(partners);
        }

        [HttpGet("history/{partnerId:guid}")]
        public async Task<IActionResult> GetHistory(Guid partnerId, [FromQuery] int take = 50)
        {
            var currentUserId = GetCurrentUserId();
            var list = await _context.ChatMessages
                .AsNoTracking()
                .Where(m => (m.FromUserId == currentUserId && m.ToUserId == partnerId) || (m.FromUserId == partnerId && m.ToUserId == currentUserId))
                .OrderByDescending(m => m.SentAt)
                .Take(Math.Clamp(take, 10, 200))
                .ToListAsync();
            return Ok(list.OrderBy(m => m.SentAt));
        }

        public class SendMessageDto
        {
            public Guid ToUserId { get; set; }
            public string Content { get; set; } = string.Empty;
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var fromUserId = GetCurrentUserId();
            var msg = new ChatMessage
            {
                Id = Guid.NewGuid(),
                FromUserId = fromUserId,
                ToUserId = dto.ToUserId,
                Content = dto.Content.Trim(),
                SentAt = DateTime.UtcNow
            };
            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();
            return Ok(msg);
        }
    }
}
