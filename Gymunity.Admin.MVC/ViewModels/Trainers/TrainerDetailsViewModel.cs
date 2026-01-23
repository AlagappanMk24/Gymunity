namespace Gymunity.Admin.MVC.ViewModels.Trainers
{
    /// <summary>
    /// ViewModel for trainer details page
    /// </summary>
    public class TrainerDetailsViewModel
    {
        public int Id { get; set; }
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
        public string? StatusImageUrl { get; set; }
        public string? StatusDescription { get; set; }
        public decimal AvailableBalance { get; set; } = 0m;
        public DateTimeOffset CreatedAt { get; set; }

        // New properties for earnings and fees
        public decimal TotalEarnings { get; set; } = 0m;
        public decimal PlatformFeesGained { get; set; } = 0m;
        public int CompletedPaymentsCount { get; set; } = 0;
        public string Currency { get; set; } = "EGP";

        // Reviews collection
        public List<TrainerReviewViewModel> Reviews { get; set; } = new();
        public int TotalReviewsCount { get; set; } = 0;

        // Packages collection
        public List<TrainerPackageViewModel> Packages { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for trainer reviews displayed on details page
    /// </summary>
    public class TrainerReviewViewModel
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string ClientName { get; set; } = null!;
        public string? ClientEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsApproved { get; set; }
    }

    /// <summary>
    /// ViewModel for trainer packages displayed on details page
    /// </summary>
    public class TrainerPackageViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public decimal PriceMonthly { get; set; }
        public decimal? PriceYearly { get; set; }
        public string Currency { get; set; } = "USD";
        public bool IsActive { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? PromoCode { get; set; }
        public int SubscriptionCount { get; set; } = 0;
    }
}
