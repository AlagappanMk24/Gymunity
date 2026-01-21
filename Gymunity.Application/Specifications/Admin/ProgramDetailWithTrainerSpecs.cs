using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Specification;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for retrieving detailed information about a specific program
    /// Includes trainer profile with user data (email, username, handle) and program weeks
    /// Used for admin program details view
    /// </summary>
    public class ProgramDetailWithTrainerSpecs : BaseSpecification<Program>
    {
        public ProgramDetailWithTrainerSpecs(int programId)
        {
            // Filter by specific program ID and ensure not soft-deleted
            Criteria = p => p.Id == programId && !p.IsDeleted;

            // Eager load trainer profile with associated user data
            AddInclude(p => p.TrainerProfile!.User);

            // Eager load program weeks (for calculating exercise count and duration)
            AddInclude(p => p.Weeks);

            // Order weeks chronologically - ascending by creation date
            AddOrderByAsc(p => p.CreatedAt);
        }
    }
}