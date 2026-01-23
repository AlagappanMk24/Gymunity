using Gymunity.Domain.Entities;
using Gymunity.Domain.Specification;
using System.Linq.Expressions;

namespace ITI.Gymunity.FP.Application.Specefications.ClientSpecification
{
    public class SubscriptionsWithClientAndProgramSpecs : BaseSpecification<Subscription>
    {
        public SubscriptionsWithClientAndProgramSpecs()
        {
            AddInclude(s => s.Package);
            AddInclude(s => s.Client);
        }

        public SubscriptionsWithClientAndProgramSpecs(Expression<Func<Subscription, bool>>? criteria) : base(criteria)
        {
            AddInclude(s => s.Package);
            AddInclude(s => s.Client);
        }
    }
}