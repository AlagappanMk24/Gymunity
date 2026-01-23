namespace Gymunity.Application.DTOs.Client
{
    public class TrainerReviewClientResponse
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string ClientUserName { get; set; } = null!;
    }
}
