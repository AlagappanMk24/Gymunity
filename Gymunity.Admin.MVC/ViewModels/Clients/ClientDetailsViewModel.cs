namespace Gymunity.Admin.MVC.ViewModels.Clients
{
    /// <summary>
    /// ViewModel for client details page showing subscriptions, payments, and profile data
    /// </summary>
    public class ClientDetailsViewModel
    {
        // User Info
        public int ProfileId { get; set; }
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsVerified { get; set; }

        // Client Profile Info
        public int? HeightCm { get; set; }
        public decimal? StartingWeightKg { get; set; }
        public string? Gender { get; set; }
        public string? Goal { get; set; }
        public string? ExperienceLevel { get; set; }

        // Statistics
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TotalPaymentsCount { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public string Currency { get; set; } = "EGP";

        // Collections
        public List<ClientSubscriptionViewModel> Subscriptions { get; set; } = new();
        public List<ClientPaymentViewModel> Payments { get; set; } = new();
    }

    /// <summary>
    /// Subscription details for client view
    /// </summary>
    public class ClientSubscriptionViewModel
    {
        public int Id { get; set; }
        public string PackageName { get; set; } = null!;
        public string TrainerHandle { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; } = "EGP";
        public bool IsAnnual { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public DateTime? CanceledAt { get; set; }

        // Computed properties
        public bool IsActive => !CanceledAt.HasValue && DateTime.UtcNow < CurrentPeriodEnd;
        public int DaysRemaining => IsActive ? (int)(CurrentPeriodEnd - DateTime.UtcNow).TotalDays : 0;
        public bool IsExpired => DateTime.UtcNow > CurrentPeriodEnd;
        public bool IsExpiringSoon => IsActive && DaysRemaining <= 7 && DaysRemaining > 0;
    }

    /// <summary>
    /// Payment details for client view
    /// </summary>
    public class ClientPaymentViewModel
    {
        public int Id { get; set; }
        public string PackageName { get; set; } = null!;
        public string TrainerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string Status { get; set; } = null!;
        public string Method { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? FailureReason { get; set; }

        // Computed properties
        public bool IsPending => Status == "Pending";
        public bool IsCompleted => Status == "Completed";
        public bool IsFailed => Status == "Failed";
        public bool IsRefunded => Status == "Refunded";
        public string StatusBadgeClass => Status switch
        {
            "Completed" => "bg-green-50 text-green-700",
            "Pending" => "bg-yellow-50 text-yellow-700",
            "Failed" => "bg-red-50 text-red-700",
            "Refunded" => "bg-blue-50 text-blue-700",
            _ => "bg-gray-50 text-gray-700"
        };
    }
}
