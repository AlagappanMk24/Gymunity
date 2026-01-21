using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for retrieving a trainer's packages with their completed payment earnings
    /// Used to calculate available balance for trainers
    /// Loads packages for a specific trainer with all related payment data
    /// </summary>
    public class TrainerPackagesWithEarningsSpecs : BaseSpecification<Package>
    {
        public TrainerPackagesWithEarningsSpecs(int trainerProfileId)
        {
            // Filter packages by trainer ID and exclude deleted packages
            Criteria = p => p.TrainerId == trainerProfileId && !p.IsDeleted;

            // Eager load subscriptions for this package
            AddInclude(p => p.Subscriptions);

            // Eager load nested payment data for earnings calculation
            // Include subscriptions with their payments
            AddInclude(q => q.Include(p => p.Subscriptions)
                .ThenInclude(s => s.Payments.Where(pm => pm.Status == PaymentStatus.Completed && !pm.IsDeleted)));

            // Order packages by creation date
            AddOrderByDesc(p => p.CreatedAt);
        }
    }
}