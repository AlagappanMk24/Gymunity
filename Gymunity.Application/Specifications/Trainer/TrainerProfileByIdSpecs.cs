using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Specification;

namespace Gymunity.Application.Specifications.Trainer
{
    public class TrainerProfileByIdSpecs : BaseSpecification<TrainerProfile>
    {
        public TrainerProfileByIdSpecs(int id)
            : base(tp => tp.Id == id)  // Criteria to filter by Id WHERE Id = @id
        {
            // Include User
            AddInclude(tp => tp.User);
            AddInclude(tp => tp.TrainerReviews);
            // Include Programs
            AddInclude(tp => tp.Programs);
        }
    }
}