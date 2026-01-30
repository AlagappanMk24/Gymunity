using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gymunity.Application.Specifications.ProgramSpecs
{
    internal class ProgramDaySpec : BaseSpecification<ProgramDay>
    {
        public ProgramDaySpec(int weekId)
            : base(d => d.ProgramWeekId == weekId)
        {
            AddInclude(d => d.Exercises);
            AddInclude(q => q.Include(d => d.Exercises).ThenInclude(pe => pe.Exercise));
        }
        public ProgramDaySpec(Expression<Func<ProgramDay, bool>>? criteria)
            : base(criteria)
        {
            AddInclude(d => d.Exercises);
            AddInclude(q => q.Include(d => d.Exercises).ThenInclude(pe => pe.Exercise));
        }
    }
}