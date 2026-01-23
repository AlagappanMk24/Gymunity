using Gymunity.Domain.Enums;

namespace Gymunity.Admin.MVC.ViewModels.Subscriptions
{
    /// <summary>
    /// DTO for subscription filtering parameters
    /// Used for AJAX filtering requests
    /// </summary>
    public class SubscriptionFilterRequest
    {
        /// <summary>
        /// Subscription status filter
        /// </summary>
        public SubscriptionStatus? Status { get; set; }

        /// <summary>
        /// Trainer ID filter (optional)
        /// </summary>
        public int? TrainerId { get; set; }

        /// <summary>
        /// Search term for client name, email, or package name
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Start date range filter
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date range filter
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Page number for pagination
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Page size for pagination
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Sort field
        /// </summary>
        public string? SortField { get; set; }

        /// <summary>
        /// Sort direction (asc/desc)
        /// </summary>
        public string? SortDirection { get; set; }
    }

    /// <summary>
    /// DTO for subscription filter response
    /// Contains filtered subscriptions and pagination info
    /// </summary>
    public class SubscriptionFilterResponse
    {
        public IEnumerable<SubscriptionFilterItem> Subscriptions { get; set; } = new List<SubscriptionFilterItem>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Subscription item for filter response
    /// Minimal data needed for table display
    /// </summary>
    public class SubscriptionFilterItem
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = null!;
        public string ClientEmail { get; set; } = null!;
        public int PackageId { get; set; }
        public string PackageName { get; set; } = null!;
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public SubscriptionStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
