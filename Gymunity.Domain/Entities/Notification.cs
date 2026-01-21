using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Enums;

namespace Gymunity.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationType Type { get; set; }
        public string? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public AppUser User { get; set; } = null!;
    }
}