using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for dashboard trainer analytics
    /// Includes packages, subscriptions, and reviews for complete trainer analysis
    /// </summary>
    public class DashboardTrainerAnalyticsSpecs : BaseSpecification<TrainerProfile>
    {
        /// <summary>
        /// Get all trainers with related data
        /// Optimized for dashboard analytics and aggregations
        /// </summary>
        public DashboardTrainerAnalyticsSpecs()
        {
            // Include user information
            AddInclude(q => q.Include(t => t.User));

            // Include packages with subscriptions
            AddInclude(q => q.Include(t => t.Packages)
                .ThenInclude(p => p.Subscriptions));

            // Include reviews
            AddInclude(q => q.Include(t => t.TrainerReviews));

            // Order by rating then by clients
            AddOrderByDesc(t => t.RatingAverage);
            AddOrderByDesc(t => t.TotalClients);

            // No filtering - include all for analytics
        }

        /// <summary>
        /// Get verified trainers only
        /// </summary>
        public static DashboardTrainerAnalyticsSpecs VerifiedTrainers()
        {
            var spec = new DashboardTrainerAnalyticsSpecs();
            spec.Criteria = t => t.IsVerified && !t.IsDeleted;
            return spec;
        }

        /// <summary>
        /// Get unverified trainers
        /// </summary>
        public static DashboardTrainerAnalyticsSpecs UnverifiedTrainers()
        {
            var spec = new DashboardTrainerAnalyticsSpecs();
            spec.Criteria = t => !t.IsVerified && !t.IsDeleted;
            return spec;
        }

        /// <summary>
        /// Get top trainers by rating
        /// </summary>
        public static DashboardTrainerAnalyticsSpecs TopTrainersByRating(int count = 5)
        {
            var spec = new DashboardTrainerAnalyticsSpecs();
            spec.Criteria = t => !t.IsDeleted;
            // Apply limit will be done in service layer
            return spec;
        }

        /// <summary>
        /// Get top trainers by client count
        /// </summary>
        public static DashboardTrainerAnalyticsSpecs TopTrainersByClients(int count = 5)
        {
            var spec = new DashboardTrainerAnalyticsSpecs();
            spec.Criteria = t => !t.IsDeleted;
            return spec;
        }
    }
}
