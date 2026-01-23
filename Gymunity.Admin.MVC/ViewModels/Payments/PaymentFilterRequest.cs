using Gymunity.Domain.Enums;

namespace Gymunity.Admin.MVC.ViewModels.Payments
{
    /// <summary>
    /// Request model for advanced payment filtering via AJAX
    /// </summary>
    public class PaymentFilterRequest
    {
        /// <summary>
        /// Filter by payment status
        /// </summary>
        public PaymentStatus? Status { get; set; }

        /// <summary>
        /// Search client by name or email
        /// </summary>
        public string? ClientSearch { get; set; }

        /// <summary>
        /// Filter by trainer profile ID
        /// </summary>
        public int? TrainerProfileId { get; set; }

        /// <summary>
        /// Minimum amount filter
        /// </summary>
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// Maximum amount filter
        /// </summary>
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// Start date for date range filter
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for date range filter
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
    }
}
