using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace sms.api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task SendPrivateMessage(string toUserId, string content)
        {
            await Clients.User(toUserId).SendAsync("ReceiveMessage", Context.UserIdentifier, content, DateTime.UtcNow);
        }

        public async Task Typing(string toUserId, bool isTyping)
        {
            await Clients.User(toUserId).SendAsync("Typing", Context.UserIdentifier, isTyping);
        }
    }
}
