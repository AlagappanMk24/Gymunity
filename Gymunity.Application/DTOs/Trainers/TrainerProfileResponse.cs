namespace Gymunity.Application.DTOs.Trainers
{
    public class TrainerProfileResponse
    {
        public int Id { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string UserName { get; set; } = null!;
        public string Handle { get; set; } = null!;
        public string Bio { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public string? VideoIntroUrl { get; set; }
        public string? BrandingColors { get; set; }

        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public decimal RatingAverage { get; set; } = 0;
        public int TotalClients { get; set; } = 0;
        public int YearsExperience { get; set; }
        public string? StatusImageUrl { get; set; }
        public string? StatusDescription { get; set; }
    }
}