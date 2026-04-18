using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace sms.api.Services
{
    /// <summary>
    /// Custom SignalR User ID Provider to extract user ID from JWT token
    /// NOTE: This enables SignalR to identify users from JWT authentication
    /// </summary>
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // Get user ID from JWT token claims
            var userId = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Also try the "sub" claim which is commonly used in JWT tokens
            if (string.IsNullOrEmpty(userId))
            {
                userId = connection.User?.FindFirst("sub")?.Value;
            }
            
            // Try the "id" claim as well (custom claim)
            if (string.IsNullOrEmpty(userId))
            {
                userId = connection.User?.FindFirst("id")?.Value;
            }
            
            return userId;
        }
    }
}
