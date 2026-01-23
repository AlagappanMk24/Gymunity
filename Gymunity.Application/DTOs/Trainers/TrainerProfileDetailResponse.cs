namespace Gymunity.Application.DTOs.Trainers
{
    public class TrainerProfileDetailResponse
    {
        public int Id { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Handle { get; set; } = null!;
        public string Bio { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public string? VideoIntroUrl { get; set; }
        public string? BrandingColors { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsSuspended { get; set; }
        public DateTime? SuspendedAt { get; set; }
        public decimal RatingAverage { get; set; }
        public int TotalClients { get; set; }
        public int YearsExperience { get; set; }

        // Status fields - only included in detail responses
        public string? StatusImageUrl { get; set; }
        public string? StatusDescription { get; set; }

        // Available balance (computed)
        public decimal AvailableBalance { get; set; } = 0m;

        // Reviews
        public List<TrainerReviewResponse> Reviews { get; set; } = [];
        public int TotalReviewsCount { get; set; }

        // Sum of ratings (for clarity): RatingAverage = RatingSum / TotalReviewsCount
        public int RatingSum { get; set; }

        // Computed average based on RatingSum / TotalReviewsCount (rounded to 2 decimals)
        public decimal RatingAverageComputed => TotalReviewsCount > 0 ? Math.Round((decimal)RatingSum / TotalReviewsCount, 2) : 0m;
    }
}