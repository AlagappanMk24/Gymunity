using Gymunity.Domain.Enums;

namespace Gymunity.Application.DTOs.Messaging
{
    public class MessageResponse
    {
        public long Id { get; set; }
        public int ThreadId { get; set; }
        public string SenderId { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string SenderProfilePhoto { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? MediaUrl { get; set; }
        public MessageType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
