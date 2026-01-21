using Gymunity.Domain.Enums;

namespace Gymunity.Application.DTOs.Notifications
{
    public class NotificationResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationType Type { get; set; }
        public string? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
