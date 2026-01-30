using Gymunity.Application.Common;
using Gymunity.Application.DTOs.User.Subscriptions;
using Gymunity.Domain.Enums;

namespace Gymunity.Application.Contracts.Services.Client
{
    public interface ISubscriptionService
    {
        Task<bool> HasActiveSubscribtionToPackageAsync(string clientId, int packageId);
        Task<ServiceResult<SubscriptionResponse>> SubscribeAsync(
            string clientId,
            SubscribePackageRequest request);
        Task<ServiceResult<SubscriptionListResponse>> GetClientSubscriptionsAsync(
           string clientId,
           SubscriptionStatus? status = null);
        Task<ServiceResult<SubscriptionResponse>> GetSubscriptionByIdAsync(
            int subscriptionId,
            string clientId);
        Task<ServiceResult<bool>> CancelSubscriptionAsync(
            int subscriptionId,
            string clientId);
        Task<ServiceResult<SubscriptionResponse>> ActivateSubscriptionAsync(
            int subscriptionId,
            string paymentTransactionId);
    }
}