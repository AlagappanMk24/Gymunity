using Gymunity.Domain.Entities;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.Subscriptions
{
    /// <summary>
    /// Check if client already has active/unpaid subscription for specific package
    /// (Used to prevent duplicate subscriptions)
    /// </summary>
    public class ClientSubscriptionForPackageSpecs : BaseSpecification<Subscription>
    {
        public ClientSubscriptionForPackageSpecs(string clientId, int packageId)
            : base(s => s.ClientId == clientId
                     && s.PackageId == packageId)
        {
            AddInclude(query => query.Include(s => s.Package));
        }
    }
}