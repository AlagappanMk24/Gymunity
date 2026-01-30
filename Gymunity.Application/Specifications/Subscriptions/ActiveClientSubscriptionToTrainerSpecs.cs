using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.Subscriptions
{
    /// <summary>
    /// Check if client has active subscription to a specific trainer
    /// </summary>
    public class ActiveClientSubscriptionToTrainerSpecs : BaseSpecification<Subscription>
    {
        public ActiveClientSubscriptionToTrainerSpecs(string clientId, int trainerId)
            : base(s => s.ClientId == clientId
                     && s.Package.TrainerId == trainerId
                     && s.Status == SubscriptionStatus.Active
                     && s.CurrentPeriodEnd > DateTime.UtcNow)
        {
            AddInclude(query => query
                .Include(s => s.Package)
                    .ThenInclude(p => p.Trainer));
        }
    }
}