using api.data.Entities;
using api.data;
namespace api.services.NotificationService
{
     public interface INotificationService
    {
        Task SendNotificationAsync(int userId, string title, string body, NotificationType type = NotificationType.Info, string? actionUrl = null);
    }
}