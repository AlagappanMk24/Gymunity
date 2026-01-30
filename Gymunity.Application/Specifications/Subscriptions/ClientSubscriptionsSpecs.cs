using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.Subscriptions
{
    /// <summary>
    /// Get all client subscriptions with optional status filter
    /// </summary>
    public class ClientSubscriptionsSpecs : BaseSpecification<Subscription>
    {
        public ClientSubscriptionsSpecs(string clientId, SubscriptionStatus? status = null)
            : base(s => s.ClientId == clientId
                     && (!status.HasValue || s.Status == status.Value))
        {
            // Include Package → Trainer → User
            AddInclude(query => query
                .Include(s => s.Package)
                    .ThenInclude(p => p.Trainer)
                        .ThenInclude(t => t.User));

            AddOrderByDesc(s => s.CreatedAt);
        }
    }
}