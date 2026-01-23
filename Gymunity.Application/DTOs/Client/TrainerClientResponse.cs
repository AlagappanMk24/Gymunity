using Gymunity.Application.DTOs.Trainers;

namespace Gymunity.Application.DTOs.Client
{
    public class TrainerClientResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Handle { get; set; } = null!;
        public string Bio { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public decimal RatingAverage { get; set; }
        public int TotalClients { get; set; }
        public int YearsExperience { get; set; }

        // New: reviews and summary
        public List<TrainerReviewResponse> Reviews { get; set; } = new();
        public int TotalReviewsCount { get; set; }
        public int RatingSum { get; set; }
        public decimal RatingAverageComputed { get; set; }
    }
}
