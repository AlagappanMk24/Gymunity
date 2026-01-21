namespace Gymunity.Admin.MVC.ViewModels.Chat
{
    public class ChatListViewModel
    {
        public IEnumerable<ChatThreadViewModel> Threads { get; set; } = new List<ChatThreadViewModel>();
        public int TotalUnreadCount { get; set; }
    }
}
