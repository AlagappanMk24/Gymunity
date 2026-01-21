namespace Gymunity.Application.DTOs.Messaging
{
    public class CreateChatThreadResponse
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = null!;
        public string TrainerId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
    }
}
