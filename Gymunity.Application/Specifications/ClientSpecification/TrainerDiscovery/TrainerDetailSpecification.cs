using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Specification;

namespace Gymunity.Application.Specifications.ClientSpecification.TrainerDiscovery
{
    public class TrainerDetailSpecification : BaseSpecification<TrainerProfile>
    {
        public TrainerDetailSpecification(string trainerId)
            : base(t => t.UserId == trainerId)
        {
            AddInclude(t => t.User);
            AddInclude(t => t.Packages);
            AddInclude(t => t.Programs);
            AddInclude(t => t.Programs.Select(p => p.Weeks));
            AddInclude(t => t.TrainerReviews);
            AddInclude(t => t.TrainerReviews.Select(r => r.Client));
        }
    }
}