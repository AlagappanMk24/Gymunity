using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gymunity.Application.Specifications.ClientSpecification
{
    public class ClientWithUserSpecs : BaseSpecification<ClientProfile>
    {
        public ClientWithUserSpecs()
        {
            AddInclude(c => c.User);
            AddInclude(c => c.Include(b => b.BodyStatLogs));
            AddInclude(c => c.Include(w => w.WorkoutLogs));
        }
        public ClientWithUserSpecs(Expression<Func<ClientProfile, bool>>? criteriaExpression) : base(criteriaExpression)
        {
            AddInclude(t => t.User);
            AddInclude(c => c.Include(b => b.BodyStatLogs));
            AddInclude(c => c.Include(w => w.WorkoutLogs));

        }
    }
}