using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for advanced subscription filtering with multiple criteria
    /// Supports filtering by status, trainer, date range, and search terms
    /// Optimized for AJAX requests and dynamic filtering
    /// </summary>
    public class SubscriptionAdvancedFilterSpecs : BaseSpecification<Subscription>
    {
        public SubscriptionAdvancedFilterSpecs(
            SubscriptionStatus? status = null,
            int? trainerId = null,
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            // Build dynamic criteria
            var criteria = BuildCriteria(status, trainerId, searchTerm, startDate, endDate);
            
            if (criteria != null)
            {
                Criteria = criteria;
            }

            // Eager load essential relationships
            AddInclude(s => s.Client);
            AddInclude(q => q.Include(s => s.Package)
                .ThenInclude(p => p.Trainer!.User));

            // Optional: Include payments for revenue calculations
            AddInclude(s => s.Payments);

            // Sort by most recent first
            AddOrderByDesc(s => s.StartDate);

            // Apply pagination
            ApplyPagination((pageNumber - 1) * pageSize, pageSize);
        }

        /// <summary>
        /// Builds dynamic criteria expression based on filter parameters
        /// </summary>
        private System.Linq.Expressions.Expression<Func<Subscription, bool>>? BuildCriteria(
            SubscriptionStatus? status,
            int? trainerId,
            string? searchTerm,
            DateTime? startDate,
            DateTime? endDate)
        {
            System.Linq.Expressions.Expression<Func<Subscription, bool>>? criteria = null;

            // Status filter
            if (status.HasValue)
            {
                criteria = s => s.Status == status.Value && !s.IsDeleted;
            }

            // Trainer filter
            if (trainerId.HasValue)
            {
                if (criteria == null)
                {
                    criteria = s => s.Package.TrainerId == trainerId.Value && !s.IsDeleted;
                }
                else
                {
                    criteria = s => s.Status == status!.Value && 
                                   s.Package.TrainerId == trainerId.Value && !s.IsDeleted;
                }
            }

            // Search term filter (client name, email, or package name)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                if (criteria == null)
                {
                    criteria = s => (EF.Functions.Like(s.Client.UserName, $"%{searchTerm}%") ||
                                    EF.Functions.Like(s.Client.Email, $"%{searchTerm}%") ||
                                    EF.Functions.Like(s.Package.Name, $"%{searchTerm}%")) && 
                                    !s.IsDeleted;
                }
                else
                {
                    var existingCriteria = criteria;
                    criteria = s => (EF.Functions.Like(s.Client.UserName, $"%{searchTerm}%") ||
                                   EF.Functions.Like(s.Client.Email, $"%{searchTerm}%") ||
                                   EF.Functions.Like(s.Package.Name, $"%{searchTerm}%")) &&
                                   !s.IsDeleted;
                }
            }

            // Date range filter
            if (startDate.HasValue && endDate.HasValue)
            {
                if (criteria == null)
                {
                    criteria = s => s.StartDate >= startDate.Value && 
                                   s.StartDate <= endDate.Value && !s.IsDeleted;
                }
                else
                {
                    criteria = s => s.StartDate >= startDate.Value && 
                                   s.StartDate <= endDate.Value && !s.IsDeleted;
                }
            }

            // Default: not deleted
            if (criteria == null)
            {
                criteria = s => !s.IsDeleted;
            }

            return criteria;
        }
    }
}