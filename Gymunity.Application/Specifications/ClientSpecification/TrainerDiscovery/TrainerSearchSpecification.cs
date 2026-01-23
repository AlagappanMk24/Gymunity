using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Specification;

namespace Gymunity.Application.Specifications.ClientSpecification.TrainerDiscovery
{
    public class TrainerSearchSpecification : BaseSpecification<TrainerProfile>
    {
        public TrainerSearchSpecification(
            string? search = null,
            int? minExperience = null,
            bool? isVerified = null,
            bool isSuspended = false)
            : base(t =>
                (!isSuspended || !t.IsSuspended) &&
                (!isVerified.HasValue || t.IsVerified == isVerified.Value) &&
                (!minExperience.HasValue || t.YearsExperience >= minExperience.Value) &&
                (string.IsNullOrEmpty(search) ||
                 t.User.FullName.Contains(search) ||
                 t.Handle.Contains(search) ||
                 t.Bio.Contains(search)))
        {
            AddInclude(t => t.User);
            AddInclude(t => t.Packages);
        }
    }
}