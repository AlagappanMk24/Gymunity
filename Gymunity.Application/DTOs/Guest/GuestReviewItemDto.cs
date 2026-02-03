namespace Gymunity.Application.DTOs.Guest
{
    public class GuestReviewResponseItem
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string ClientUserName { get; set; } = string.Empty;
    }
}
