namespace Gymunity.Admin.MVC.ViewModels.Chat
{
    public class ChatMessageViewModel
    {
        public long Id { get; set; }
        public int ThreadId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? SenderProfilePhoto { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
        public int Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
