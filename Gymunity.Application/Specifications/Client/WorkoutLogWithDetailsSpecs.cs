using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Specification;
using System.Linq.Expressions;

namespace Gymunity.Application.Specifications.Client
{
    public class WorkoutLogWithDetailsSpecs : BaseSpecification<WorkoutLog>
    {
        public WorkoutLogWithDetailsSpecs()
        {
            AddInclude(w => w.ClientProfile);
            AddInclude(w => w.ProgramDay);
            AddOrderByDesc(w => w.CompletedAt);
        }

        public WorkoutLogWithDetailsSpecs(Expression<Func<WorkoutLog, bool>> criteria)
        : base(criteria)
        {
            AddInclude(w => w.ClientProfile);
            AddInclude(w => w.ProgramDay);
            AddOrderByDesc(w => w.CompletedAt);
        }
    }
}