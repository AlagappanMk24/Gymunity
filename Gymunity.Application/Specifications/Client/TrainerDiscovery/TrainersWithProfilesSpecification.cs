using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Specification;

namespace Gymunity.Application.Specifications.Client.TrainerDiscovery
{
    public class TrainersWithProfilesSpecification : BaseSpecification<TrainerProfile>
    {
        public TrainersWithProfilesSpecification(bool? isVerified = null, bool? isSuspended = false)
            : base(t => (!isVerified.HasValue || t.IsVerified == isVerified.Value) &&
                        (!isSuspended.HasValue || t.IsSuspended == isSuspended.Value))
        {
            AddInclude(t => t.User);
            AddInclude(t => t.Packages);
            AddInclude(t => t.Programs);

            AddOrderByDesc(t => t.RatingAverage);
        }
    }
}