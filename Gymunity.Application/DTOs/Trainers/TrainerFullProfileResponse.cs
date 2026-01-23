namespace Gymunity.Application.DTOs.Trainers
{
    public class TrainerFullProfileResponse
    {
        // ===== User Data =====
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? ProfilePhotoUrl { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // ===== TrainerProfile Data =====
        public int TrainerProfileId { get; set; }
        public string Handle { get; set; } = null!;
        public string Bio { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public string? VideoIntroUrl { get; set; }
        public string? BrandingColors { get; set; }
        public bool IsProfileVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsSuspended { get; set; }
        public DateTime? SuspendedAt { get; set; }
        public decimal RatingAverage { get; set; }
        public int TotalClients { get; set; }
        public int YearsExperience { get; set; }
        public string? StatusImageUrl { get; set; }
        public string? StatusDescription { get; set; }
        public DateTimeOffset ProfileCreatedAt { get; set; }
        public DateTimeOffset? ProfileUpdatedAt { get; set; }
    }
}