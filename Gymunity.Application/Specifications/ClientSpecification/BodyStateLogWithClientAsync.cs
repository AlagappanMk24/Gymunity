using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gymunity.Application.Specifications.ClientSpecification
{
    public class BodyStateLogWithClientAsync : BaseSpecification<BodyStatLog>
    {
        public BodyStateLogWithClientAsync()
        {
            AddInclude(b => b.ClientProfile);
            AddInclude(q => q.Include(tp => tp.ClientProfile).ThenInclude(p => p.BodyStatLogs));
        }

        public BodyStateLogWithClientAsync(Expression<Func<BodyStatLog, bool>>? criteria) : base(criteria) 
        {
            AddInclude(b => b.ClientProfile);
        }
    }
}