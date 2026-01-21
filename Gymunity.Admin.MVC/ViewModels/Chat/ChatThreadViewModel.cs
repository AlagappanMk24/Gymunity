namespace Gymunity.Admin.MVC.ViewModels.Chat
{
    public class ChatThreadViewModel
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public string? ClientProfilePhoto { get; set; }
        public string TrainerId { get; set; } = string.Empty;
        public string? TrainerName { get; set; }
        public string? TrainerProfilePhoto { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public bool IsPriority { get; set; }
        public int UnreadCount { get; set; }
        public IEnumerable<ChatMessageViewModel> Messages { get; set; } = new List<ChatMessageViewModel>();
    }
}
