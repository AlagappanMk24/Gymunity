using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.ClientSpecification
{
    internal class ActiveSubscriptionsWithProgramsByUserIdSpecification : BaseSpecification<Subscription>
    {
        public ActiveSubscriptionsWithProgramsByUserIdSpecification(string userId)
            : base(s => s.ClientId == userId && s.Status == SubscriptionStatus.Active)
        {
            AddInclude(q => q.Include(s => s.Package)
                .ThenInclude(p => p.PackagePrograms)
                .ThenInclude(p => p.Program));
        }
    }
}