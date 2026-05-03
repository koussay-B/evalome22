using api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace api.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.User == null)
            {
                throw new HubException("Cannot get current user claim");
            }

            int userId = Context.User.GetUserId();
            bool isOnline = await _tracker.UserConnected(userId, Context.ConnectionId);
            
            int[] currentUsers = await _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User == null)
            {
                throw new HubException("Cannot get current user claim");
            }

            int userId = Context.User.GetUserId(); // Assumes GetUserId() exists
            bool isOffline = await _tracker.UserDisconnected(userId, Context.ConnectionId);
            
            int[] currentUsers = await _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

            await base.OnDisconnectedAsync(exception);
        }
    }
}