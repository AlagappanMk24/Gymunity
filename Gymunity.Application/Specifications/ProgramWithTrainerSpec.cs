using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications
{
    public class ProgramWithTrainerSpec : BaseSpecification<Program>
    {
        public ProgramWithTrainerSpec(string? searchTerm = null)
        {
            AddInclude(q => q.Include(p => p.TrainerProfile).ThenInclude(tp => tp.User));

            if (!string.IsNullOrEmpty(searchTerm))
            {
                Criteria = p => p.Title.Contains(searchTerm) ||
                p.TrainerProfile != null && p.TrainerProfile.User.FullName.Contains(searchTerm) ||
                p.TrainerProfile != null && p.TrainerProfile.Handle.Contains(searchTerm);
            }

            AddOrderByDesc(p => p.CreatedAt);
            ApplyPagination(0, 50);
        }
    }
}