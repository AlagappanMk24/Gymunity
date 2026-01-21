using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for advanced payment filtering with multiple criteria
    /// Supports status, client, trainer, amount range, and date range filters
    /// Includes eager loading of all related entities
    /// </summary>
    public class PaymentAdvancedFilterSpecs : BaseSpecification<Payment>
    {
        public PaymentAdvancedFilterSpecs(
            PaymentStatus? status = null,
            string? clientSearch = null,
            int? trainerProfileId = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            // Build dynamic criteria
            Expression<Func<Payment, bool>>? criteria = p => !p.IsDeleted;

            // Status filter
            if (status.HasValue)
            {
                criteria = criteria != null
                    ? p => !p.IsDeleted && p.Status == status.Value
                    : p => p.Status == status.Value;
            }

            // Client search filter
            if (!string.IsNullOrWhiteSpace(clientSearch))
            {
                var search = clientSearch.ToLower();
                var searchCriteria = (Expression<Func<Payment, bool>>)(p =>
                    p.Subscription != null && p.Subscription.Client != null && (
                        p.Subscription.Client.FullName.ToLower().Contains(search) ||
                        p.Subscription.Client.Email.ToLower().Contains(search)
                    ));

                if (criteria != null)
                {
                    var existingCriteria = criteria;
                    criteria = p => existingCriteria.Compile().Invoke(p) && searchCriteria.Compile().Invoke(p);
                }
                else
                {
                    criteria = searchCriteria;
                }
            }

            // Trainer filter
            if (trainerProfileId.HasValue)
            {
                var trainerId = trainerProfileId.Value;
                var trainerCriteria = (Expression<Func<Payment, bool>>)(p =>
                    p.Subscription != null && p.Subscription.Package != null &&
                    p.Subscription.Package.Trainer != null && p.Subscription.Package.Trainer.Id == trainerId);

                if (criteria != null)
                {
                    var existingCriteria = criteria;
                    criteria = p => existingCriteria.Compile().Invoke(p) && trainerCriteria.Compile().Invoke(p);
                }
                else
                {
                    criteria = trainerCriteria;
                }
            }

            // Min amount filter
            if (minAmount.HasValue)
            {
                var min = minAmount.Value;
                var minCriteria = (Expression<Func<Payment, bool>>)(p => p.Amount >= min);

                if (criteria != null)
                {
                    var existingCriteria = criteria;
                    criteria = p => existingCriteria.Compile().Invoke(p) && minCriteria.Compile().Invoke(p);
                }
                else
                {
                    criteria = minCriteria;
                }
            }

            // Max amount filter
            if (maxAmount.HasValue)
            {
                var max = maxAmount.Value;
                var maxCriteria = (Expression<Func<Payment, bool>>)(p => p.Amount <= max);

                if (criteria != null)
                {
                    var existingCriteria = criteria;
                    criteria = p => existingCriteria.Compile().Invoke(p) && maxCriteria.Compile().Invoke(p);
                }
                else
                {
                    criteria = maxCriteria;
                }
            }

            // Start date filter
            if (startDate.HasValue)
            {
                var start = startDate.Value;
                var startCriteria = (Expression<Func<Payment, bool>>)(p => p.CreatedAt >= start);

                if (criteria != null)
                {
                    var existingCriteria = criteria;
                    criteria = p => existingCriteria.Compile().Invoke(p) && startCriteria.Compile().Invoke(p);
                }
                else
                {
                    criteria = startCriteria;
                }
            }

            // End date filter
            if (endDate.HasValue)
            {
                var end = endDate.Value.AddDays(1); // Include the entire last day
                var endCriteria = (Expression<Func<Payment, bool>>)(p => p.CreatedAt < end);

                if (criteria != null)
                {
                    var existingCriteria = criteria;
                    criteria = p => existingCriteria.Compile().Invoke(p) && endCriteria.Compile().Invoke(p);
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
            AddInclude(q => q.Include(p => p.Subscription)
                .ThenInclude(s => s.Client));

            AddInclude(q => q.Include(p => p.Subscription)
                .ThenInclude(s => s.Package)
                .ThenInclude(pkg => pkg.Trainer)
                .ThenInclude(t => t.User));

            // Sort by newest first
            AddOrderByDesc(p => p.CreatedAt);

            // Apply pagination
            ApplyPagination((pageNumber - 1) * pageSize, pageSize);
        }
    }
}
