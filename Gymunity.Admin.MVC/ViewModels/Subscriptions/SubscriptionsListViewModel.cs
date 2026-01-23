using Gymunity.Application.DTOs.User.Subscriptions;
using Gymunity.Domain.Enums;

namespace Gymunity.Admin.MVC.ViewModels.Subscriptions
{
    public class SubscriptionsListViewModel
    {
        public List<SubscriptionResponse> Subscriptions { get; set; } = new List<SubscriptionResponse>();
        
        public int PageNumber { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
        
        public int TotalCount { get; set; } = 0;
        
        public SubscriptionStatus? StatusFilter { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

        /// <summary>
        /// Check if there are more pages
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Check if there are previous pages
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Get total value of displayed subscriptions
        /// </summary>
        public decimal TotalValue => Subscriptions.Sum(s => s.AmountPaid);
    }
}