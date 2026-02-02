using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Specification;

namespace Gymunity.Application.Specifications.Trainer
{
    public class TrainerByUserIdSpecs : BaseSpecification<TrainerProfile>
    {
        public TrainerByUserIdSpecs(string userId)
            : base(t => t.UserId == userId)  // ✅ UserId مش Id
        {
            AddInclude(t => t.User);  // لو محتاج بيانات الـ User
        }
    }
}