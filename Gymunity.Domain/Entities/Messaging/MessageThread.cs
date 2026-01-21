using Gymunity.Domain.Entities.Identity;

namespace Gymunity.Domain.Entities.Messaging
{
    public class MessageThread : BaseEntity
    {
        public string ClientId { get; set; } = null!;
        public string TrainerId { get; set; } = null!;
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
        public bool IsPriority { get; set; } = false;

        public AppUser Client { get; set; } = null!;
        public AppUser Trainer { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = [];
    }
}
