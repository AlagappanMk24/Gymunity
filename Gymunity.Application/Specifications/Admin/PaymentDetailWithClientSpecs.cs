using Gymunity.Domain.Entities;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for retrieving detailed payment information for admin management
    /// Includes client, subscription, package, and trainer information
    /// Optimized for admin details view
    /// </summary>
    public class PaymentDetailWithClientSpecs : BaseSpecification<Payment>
    {
        public PaymentDetailWithClientSpecs(int paymentId)
        {
            // Filter by specific payment ID
            Criteria = p => p.Id == paymentId && !p.IsDeleted;

            // Eager load subscription with all related data
            AddInclude(q => q.Include(p => p.Subscription)
                .ThenInclude(s => s.Client));

            // Eager load package with trainer and user data
            AddInclude(q => q.Include(p => p.Subscription)
                .ThenInclude(s => s.Package)
                .ThenInclude(pkg => pkg.Trainer)
                .ThenInclude(t => t.User));

            // Order by newest first
            AddOrderByDesc(p => p.CreatedAt);
        }
    }
}
