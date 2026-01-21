using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for retrieving pending trainer reviews awaiting admin approval
    /// </summary>
    public class PendingReviewsSpecs : BaseSpecification<TrainerReview>
    {
        public PendingReviewsSpecs(int pageNumber = 1, int pageSize = 10)
        {
            // Get only pending (non-approved) reviews
            Criteria = r => !r.IsApproved && !r.IsDeleted;

            // Eager load related data
            AddInclude(r => r.Client);
            AddInclude(r => r.Trainer);
            AddInclude(q => q.Include(r => r.Trainer).ThenInclude(t => t.User));

            // Sort by newest first
            AddOrderByDesc(r => r.CreatedAt);

            // Apply pagination
            ApplyPagination((pageNumber - 1) * pageSize, pageSize);
        }
    }
}