using api.data.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.data.Repositories.NotificationRepo
{
    public class NotificationRepository(ApplicationContext context)
     : BaseRepository<Notification>(context), INotificationRepository
    {
        public async Task<IList<Notification>> GetByUserId(int userId)
        {
            return await _dbContext.Set<Notification>()
                .Where(n => n.Enable && n.UserId == userId)
                .OrderByDescending(n => n.Date)
                .Take(50)
                .ToListAsync();
        }

        public async Task MarkAllReadByUserId(int userId)
        {
            var notifications = await _dbContext.Set<Notification>()
                .Where(n => n.Enable && n.UserId == userId && !n.Received)
                .ToListAsync();

            foreach (var n in notifications)
                n.Received = true;

            await _dbContext.SaveChangesAsync();
        }
    }
}