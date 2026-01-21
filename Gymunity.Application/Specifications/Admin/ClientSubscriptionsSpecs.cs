using Gymunity.Domain.Entities;
using Gymunity.Domain.Specification;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for retrieving client subscriptions with all related data
    /// Includes Package, Trainer, and Trainer User information
    /// </summary>
    public class ClientSubscriptionsSpecs : BaseSpecification<Subscription>
    {
        public ClientSubscriptionsSpecs(string clientId)
        {
            // Filter subscriptions for this client
            Criteria = s => s.ClientId == clientId;

            // Include all related data for complete subscription information
            AddInclude(s => s.Package);
            AddInclude(s => s.Package.Trainer);
            AddInclude(s => s.Package.Trainer.User);

            // Sort by newest subscriptions first
            AddOrderByDesc(s => s.StartDate);
        }
    }
}