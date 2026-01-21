using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for filtering and retrieving payment records with pagination
    /// Supports filtering by payment status and date range
    /// </summary>
    public class PaymentFilterSpecs : BaseSpecification<Payment>
    {
        public PaymentFilterSpecs(
            PaymentStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            // Build criteria based on filters
            Expression<Func<Payment, bool>>? criteria = null;

            // Status filter
            if (status.HasValue)
            {
                criteria = p => p.Status == status.Value;
            }

            // Start date filter
            if (startDate.HasValue)
            {
                var start = startDate.Value;
                var startCriteria = (Expression<Func<Payment, bool>>)(p => p.CreatedAt >= start);

                if (criteria != null)
                {
                    var existingCriteria = criteria;
                    criteria = p => startCriteria.Compile().Invoke(p) && existingCriteria.Compile().Invoke(p);
                }
                else
                {
                    criteria = startCriteria;
                }
            }

            // End date filter
            if (endDate.HasValue)
            {
                var end = endDate.Value;
                var endCriteria = (Expression<Func<Payment, bool>>)(p => p.CreatedAt <= end);

                if (criteria != null)
                {
                    var existingCriteria = criteria;
                    criteria = p => endCriteria.Compile().Invoke(p) && existingCriteria.Compile().Invoke(p);
                }
                else
                {
                    criteria = endCriteria;
                }
            }

            if (criteria != null)
            {
                Criteria = criteria;
            }

            // Eager load related data
            AddInclude(p => p.Subscription);
            AddInclude(q => q.Include(p => p.Subscription).ThenInclude(s => s.Client));
            AddInclude(q => q.Include(p => p.Subscription).ThenInclude(s => s.Package));

            // Sort by newest first
            AddOrderByDesc(p => p.CreatedAt);

            // Apply pagination
            ApplyPagination((pageNumber - 1) * pageSize, pageSize);
        }
    }
}