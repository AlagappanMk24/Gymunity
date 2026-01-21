using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for dashboard payment analytics
    /// Includes subscription and trainer information for complete payment analysis
    /// </summary>
    public class DashboardPaymentAnalyticsSpecs : BaseSpecification<Payment>
    {
        /// <summary>
        /// Get all payments with related subscription, package, and trainer data
        /// Optimized for dashboard analytics and aggregations
        /// </summary>
        public DashboardPaymentAnalyticsSpecs()
        {
            // Include subscription with client information
            AddInclude(q => q.Include(p => p.Subscription)
                .ThenInclude(s => s.Client));

            // Include package and trainer information
            AddInclude(q => q.Include(p => p.Subscription)
                .ThenInclude(s => s.Package)
                .ThenInclude(pkg => pkg.Trainer));

            // Order by most recent first
            AddOrderByDesc(p => p.CreatedAt);

            // No filtering of deleted records - include all for accurate analytics
        }

        /// <summary>
        /// Get payment analytics for a specific date range
        /// </summary>
        public DashboardPaymentAnalyticsSpecs(DateTime startDate, DateTime endDate)
            : this()
        {
            Criteria = p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && !p.IsDeleted;
        }

        /// <summary>
        /// Get completed payments only
        /// </summary>
        public static DashboardPaymentAnalyticsSpecs CompletedPayments()
        {
            var spec = new DashboardPaymentAnalyticsSpecs();
            spec.Criteria = p => p.Status == PaymentStatus.Completed && !p.IsDeleted;
            return spec;
        }

        /// <summary>
        /// Get failed payments for analysis
        /// </summary>
        public static DashboardPaymentAnalyticsSpecs FailedPayments()
        {
            var spec = new DashboardPaymentAnalyticsSpecs();
            spec.Criteria = p => p.Status == PaymentStatus.Failed && !p.IsDeleted;
            return spec;
        }

        /// <summary>
        /// Get pending payments
        /// </summary>
        public static DashboardPaymentAnalyticsSpecs PendingPayments()
        {
            var spec = new DashboardPaymentAnalyticsSpecs();
            spec.Criteria = p => p.Status == PaymentStatus.Pending && !p.IsDeleted;
            return spec;
        }
    }
}
