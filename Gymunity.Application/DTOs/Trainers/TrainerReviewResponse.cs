namespace Gymunity.Application.DTOs.Trainers
{
    public class TrainerReviewResponse
    {
        public int Id { get; set; }
        public int TrainerId { get; set; }
        public int ClientId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}