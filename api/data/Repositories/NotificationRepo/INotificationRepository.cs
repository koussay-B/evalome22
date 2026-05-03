using api.data.Entities;

namespace api.data.Repositories.NotificationRepo
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<IList<Notification>> GetByUserId(int userId);
        Task MarkAllReadByUserId(int userId);
    }
}