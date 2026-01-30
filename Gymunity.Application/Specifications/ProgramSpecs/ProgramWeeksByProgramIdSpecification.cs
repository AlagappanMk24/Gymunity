using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Specification;

namespace Gymunity.Application.Specifications.ProgramSpecs
{
    public class ProgramWeeksByProgramIdSpecification : BaseSpecification<ProgramWeek>
    {
        public ProgramWeeksByProgramIdSpecification(int programId)
            : base(pw => pw.ProgramId == programId)
        {
            AddInclude(pw => pw.Days);
        }
    }
}