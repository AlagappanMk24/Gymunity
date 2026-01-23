using Gymunity.Application.DTOs.User.Subscriptions;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain.Enums;

namespace Gymunity.Application.Contracts.Services.Admin
{
    public interface ISubscriptionAdminService
    {
        Task<IEnumerable<SubscriptionResponse>> GetAllSubscriptionsAsync(
           SubscriptionFilterSpecs specs);
        Task<SubscriptionResponse?> GetSubscriptionByIdAsync(int subscriptionId);
        Task<int> GetSubscriptionCountAsync(SubscriptionFilterSpecs specs);
        Task<IEnumerable<SubscriptionResponse>> GetActiveSubscriptionsAsync(
            int pageNumber = 1,
            int pageSize = 10);
        Task<IEnumerable<SubscriptionResponse>> GetInactiveSubscriptionsAsync(
            int pageNumber = 1,
            int pageSize = 10);
        Task<IEnumerable<SubscriptionResponse>> GetUnpaidSubscriptionsAsync(
            int pageNumber = 1,
            int pageSize = 10);
        Task<bool> CancelSubscriptionAsync(int subscriptionId, string reason = "");

        Task<int> GetActiveSubscriptionCountAsync();
        Task<IEnumerable<SubscriptionResponse>> GetExpiringSoonSubscriptionsAsync();
        Task<SubscriptionResponse?> GetSubscriptionDetailsWithTrainerAsync(int subscriptionId);
        Task<IEnumerable<SubscriptionResponse>> GetSubscriptionsWithAdvancedFilterAsync(
            SubscriptionStatus? status = null,
            int? trainerId = null,
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 10);
        Task<int> GetSubscriptionCountWithAdvancedFilterAsync(
            SubscriptionStatus? status = null,
            int? trainerId = null,
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<(int activeCount, int unpaidCount, int canceledCount, decimal totalRevenue)> GetSubscriptionStatsAsync();
    }
}
