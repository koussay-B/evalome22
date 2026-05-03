using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using api.data.Entities;

namespace api.data.Entities
{
    public enum NotificationType
    {
        Info = 0,
        Success = 1,
        Warning = 2,
        Danger = 3
    }

    [Table("notifications")]
    public class Notification : BaseEntity
    {
        public required string Title { get; set; }
        public required string Body { get; set; }
        public required DateTime Date { get; set; }
        public bool Received { get; set; } = false;
        public required int UserId { get; set; }
        [JsonIgnore]
        public AppUser? User { get; set; }

        public NotificationType Type { get; set; } = NotificationType.Info;

        public string? ActionUrl { get; set; }
    }
}