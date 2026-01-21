using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for filtering and retrieving subscription records with pagination
    /// Supports filtering by subscription status
    /// </summary>
    public class SubscriptionFilterSpecs : BaseSpecification<Subscription>
    {
        public SubscriptionFilterSpecs(
            SubscriptionStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            // Apply criteria filters
            if (status.HasValue)
            {
                Criteria = s => s.Status == status.Value;
            }

            // Eager load related data
            AddInclude(s => s.Client);
            AddInclude(s => s.Package);
            AddInclude(q => q.Include(s => s.Package).ThenInclude(p => p.Trainer));

            // Sort by newest first
            AddOrderByDesc(s => s.StartDate);

            // Apply pagination
            ApplyPagination((pageNumber - 1) * pageSize, pageSize);
        }
    }
}