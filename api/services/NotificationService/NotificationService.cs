using api.data.Entities;
using api.data.Repositories;
using api.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace api.services.NotificationService
{
    public class NotificationService(IHubContext<PresenceHub> hubContext, PresenceTracker presenceTracker, IRepositoryWrapper repositoryWrapper)
 : INotificationService
    {
        private readonly IHubContext<PresenceHub> _hubContext = hubContext;
        private readonly PresenceTracker _presenceTracker = presenceTracker;
        private readonly IRepositoryWrapper _repositoryWrapper = repositoryWrapper;



        public async Task SendNotificationAsync(int userId, string title, string body, NotificationType type = NotificationType.Info, string? actionUrl = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                Date = DateTime.UtcNow,
                Received = false,
                Type = type,
                ActionUrl = actionUrl
            };
            var connectionIds = await _presenceTracker.GetConnectionsForUser(userId);
            if (connectionIds.Count > 0)
            {
                await _hubContext.Clients.Clients(connectionIds)
                    .SendAsync("ReceiveNotification", notification);
                notification.Received = true;
            }

            await _repositoryWrapper.Notification.Create(notification);
        }
    }
}