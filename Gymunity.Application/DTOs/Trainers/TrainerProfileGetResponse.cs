namespace Gymunity.Application.DTOs.Trainers
{
    public class TrainerProfileGetResponse
    {
        public int Id { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Handle { get; set; } = null!;
        public string Bio { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public string? VideoIntroUrl { get; set; }
        public string? BrandingColors { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public decimal RatingAverage { get; set; }
        public int TotalClients { get; set; }
        public int YearsExperience { get; set; }

        public string? StatusImageUrl { get; set; }
        public string? StatusDescription { get; set; }

        // Include trainer's programs if needed
        public IEnumerable<Program.ProgramGetAllResponse>? Programs { get; set; }
    }
}