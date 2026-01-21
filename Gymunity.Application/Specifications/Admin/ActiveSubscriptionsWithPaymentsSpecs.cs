using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for retrieving active subscriptions with payments for revenue calculation
    /// Optimized for statistics and reporting
    /// </summary>
    public class ActiveSubscriptionsWithPaymentsSpecs : BaseSpecification<Subscription>
    {
        public ActiveSubscriptionsWithPaymentsSpecs()
        {
            // Filter for active subscriptions only
            Criteria = s => s.Status == SubscriptionStatus.Active && !s.IsDeleted;

            // Eager load payments for revenue calculation
            AddInclude(s => s.Payments);

            // Order by most recent
            AddOrderByDesc(s => s.StartDate);
        }
    }
}
