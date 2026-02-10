using Gymunity.Admin.MVC.Services.Interfaces;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.Services.Admin;
using Gymunity.Domain.Entities;

namespace Gymunity.Admin.MVC.Services
{
    /// <summary>
    /// Handles subscription-related notifications
    /// Subscribes to events from SubscriptionAdminService and sends notifications to admins
    /// </summary>
    public class SubscriptionNotificationService : ISubscriptionNotificationService
    {
        private readonly IAdminNotificationService _notificationService;
        private readonly ISubscriptionAdminService _subscriptionAdminService;
        private readonly IAdminUserResolverService _adminUserResolver;
        private readonly ILogger<SubscriptionNotificationService> _logger;

        public SubscriptionNotificationService(
            IAdminNotificationService notificationService,
            ISubscriptionAdminService subscriptionAdminService,
            IAdminUserResolverService adminUserResolver,
            ILogger<SubscriptionNotificationService> logger)
        {
            _notificationService = notificationService;
            _subscriptionAdminService = subscriptionAdminService;
            _adminUserResolver = adminUserResolver;
            _logger = logger;

            // ✅ Subscribe to events from SubscriptionAdminService
            _subscriptionAdminService.SubscriptionCancelledByAdminAsync += OnSubscriptionCancelledByAdminAsync;
            _subscriptionAdminService.SubscriptionCreatedAsync += OnSubscriptionCreatedAsync;
        }

        /// <summary>
        /// Handle subscription cancellation event
        /// Creates notification to admins about subscription cancellation
        /// </summary>
        private async Task OnSubscriptionCancelledByAdminAsync(int subscriptionId, Subscription subscription)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about subscription cancellation {SubscriptionId}", subscriptionId);
                    return;
                }

                // Client is AppUser directly in Subscription
                var clientName = subscription.Client?.FullName ?? "Unknown Client";
                // Trainer is accessed through Package -> Trainer
                var trainerName = subscription.Package?.Trainer?.User?.FullName ?? "Unknown Trainer";
                
                await _notificationService.NotifySubscriptionCancelledAsync(
                    admin.Id,
                    clientName,
                    trainerName,
                    subscription.Id.ToString());

                _logger.LogInformation("Admin notified of subscription cancellation {SubscriptionId}", subscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send subscription cancellation notification for subscription {SubscriptionId}", subscriptionId);
                // Non-blocking - don't rethrow
            }
        }

        /// <summary>
        /// Handle subscription creation event
        /// Creates notification to admins about new subscription
        /// </summary>
        private async Task OnSubscriptionCreatedAsync(int subscriptionId, Subscription subscription)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about new subscription {SubscriptionId}", subscriptionId);
                    return;
                }

                // Client is AppUser directly in Subscription
                var clientName = subscription.Client?.FullName ?? "Unknown Client";
                // Trainer is accessed through Package -> Trainer
                var trainerName = subscription.Package?.Trainer?.User?.FullName ?? "Unknown Trainer";
                
                await _notificationService.NotifyNewSubscriptionAsync(
                    admin.Id,
                    clientName,
                    trainerName,
                    subscription.Id.ToString());

                _logger.LogInformation("Admin notified of new subscription {SubscriptionId}", subscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send new subscription notification for subscription {SubscriptionId}", subscriptionId);
                // Non-blocking - don't rethrow
            }
        }
    }
}
