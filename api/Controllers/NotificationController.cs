using api.data.Entities;
using api.data.Repositories;
using api.Extensions;
using api.services.NotificationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    // [Authorize]
    public class NotificationController(IRepositoryWrapper repo, INotificationService notificationService) : BaseApiController
    {
        private readonly IRepositoryWrapper _repo = repo;
        private readonly INotificationService _notificationService = notificationService;

        // GET /api/notification — get current user's notifications (latest 50)
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            int userId = User.GetUserId();
            var notifications = await _repo.Notification.GetByUserId(userId);
            return Ok(notifications);
        }

        // PATCH /api/notification/{id}/read — mark one notification as read
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            int userId = User.GetUserId();
            var notification = await _repo.Notification.Get(id);

            if (notification == null || notification.UserId != userId)
                return NotFound();

            notification.Received = true;
            await _repo.Notification.Update(notification);
            return NoContent();
        }

        // PATCH /api/notification/read-all — mark all notifications as read
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            int userId = User.GetUserId();
            await _repo.Notification.MarkAllReadByUserId(userId);
            return NoContent();
        }

        // POST /api/notification/test/{userId} — send a test notification (admin only)
        [HttpPost("test/{userId}")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendTestNotification(int userId, [FromBody] TestNotificationRequest request)
        {
            await _notificationService.SendNotificationAsync(
                userId,
                request.Title ?? "Test Notification",
                request.Body ?? "This is a test notification.",
                request.Type,
                request.ActionUrl
            );
            return Ok(new { message = "Test notification sent." });
        }
    }

    public class TestNotificationRequest
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Info;
        public string? ActionUrl { get; set; }
    }
}
