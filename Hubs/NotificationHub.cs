using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace sms.api.Hubs;

/// <summary>
/// Real-time inbox delivery. Server pushes InboxNotification to Clients.User(userId).
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    public override Task OnConnectedAsync() => base.OnConnectedAsync();
}
