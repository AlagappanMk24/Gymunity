using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gymunity.Application.Specifications.Trainer
{
    internal class TrainerWithUsersAndProgramsSpecs : BaseSpecification<TrainerProfile>
    {
        public TrainerWithUsersAndProgramsSpecs()
        {
            AddInclude(t => t.User);
            //AddInclude(tp => tp.Programs);
            AddInclude(q => q.Include(tp => tp.Programs).ThenInclude(p => p.Weeks));
            // include trainer reviews for detail endpoints
            AddInclude(tp => tp.TrainerReviews);
        }

        public TrainerWithUsersAndProgramsSpecs(Expression<Func<TrainerProfile, bool>>? criteria) : base(criteria)
        {
            AddInclude(t => t.User);
            //AddInclude(tp => tp.Programs);
            AddInclude(q => q.Include(tp => tp.Programs).ThenInclude(p => p.Weeks));
            // include trainer reviews for detail endpoints
            AddInclude(tp => tp.TrainerReviews);
        }
    }
}