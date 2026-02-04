using Gymunity.Application.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace Gymunity.Infrastructure.ExternalServices
{
    /// <summary>
    /// Event-driven admin notification publisher
    /// Publishes payment and subscription events for admin notification handlers to subscribe to
    /// Decouples Infrastructure from MVC Admin layer
    /// </summary>
    public class AdminNotificationPublisher : IAdminNotificationPublisher
    {
        private readonly ILogger<AdminNotificationPublisher> _logger;

        // ✅ Observable events - other services can subscribe to these
        public event Func<decimal, string, string, Task>? OnNewPaymentAsync;
        public event Func<decimal, string, string, string, Task>? OnPaymentFailureAsync;
        public event Func<string, string, string, string, Task>? OnNewSubscriptionAsync;
        public event Func<string, string, string, string, Task>? OnSubscriptionCancelledAsync;
        public event Func<string, string, DateTime, string, Task>? OnSubscriptionExpiringAsync;

        public AdminNotificationPublisher(ILogger<AdminNotificationPublisher> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Publish new payment notification
        /// </summary>
        public async Task NotifyNewPaymentAsync(decimal amount, string clientName, string paymentId)
        {
            try
            {
                _logger.LogInformation(
                    "📢 Publishing admin notification: New Payment | Amount: {Amount}, Client: {ClientName}, PaymentID: {PaymentId}",
                    amount, clientName, paymentId);

                // Raise event for subscribers (AdminNotificationService)
                if (OnNewPaymentAsync != null)
                {
                    await OnNewPaymentAsync(amount, clientName, paymentId);
                }
                else
                {
                    _logger.LogWarning("No subscribers for OnNewPaymentAsync event");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing new payment notification");
                // Don't rethrow - notification failure shouldn't break webhook processing
            }
        }

        /// <summary>
        /// Publish payment failure notification
        /// </summary>
        public async Task NotifyPaymentFailureAsync(decimal amount, string clientName, string failureReason, string paymentId)
        {
            try
            {
                _logger.LogWarning(
                    "📢 Publishing admin notification: Payment Failure | Amount: {Amount}, Client: {ClientName}, Reason: {Reason}, PaymentID: {PaymentId}",
                    amount, clientName, failureReason, paymentId);

                // Raise event for subscribers
                if (OnPaymentFailureAsync != null)
                {
                    await OnPaymentFailureAsync(amount, clientName, failureReason, paymentId);
                }
                else
                {
                    _logger.LogWarning("No subscribers for OnPaymentFailureAsync event");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing payment failure notification");
                // Don't rethrow
            }
        }

        /// <summary>
        /// Publish new subscription notification
        /// </summary>
        public async Task NotifyNewSubscriptionAsync(string clientName, string trainerName, string packageName, string subscriptionId)
        {
            try
            {
                _logger.LogInformation(
                    "📢 Publishing admin notification: New Subscription | Client: {ClientName}, Trainer: {TrainerName}, Package: {PackageName}, SubscriptionID: {SubscriptionId}",
                    clientName, trainerName, packageName, subscriptionId);

                // Raise event for subscribers
                if (OnNewSubscriptionAsync != null)
                {
                    await OnNewSubscriptionAsync(clientName, trainerName, packageName, subscriptionId);
                }
                else
                {
                    _logger.LogWarning("No subscribers for OnNewSubscriptionAsync event");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing new subscription notification");
                // Don't rethrow
            }
        }

        /// <summary>
        /// Publish subscription cancelled notification
        /// </summary>
        public async Task NotifySubscriptionCancelledAsync(string clientName, string trainerName, string reason, string subscriptionId)
        {
            try
            {
                _logger.LogInformation(
                    "📢 Publishing admin notification: Subscription Cancelled | Client: {ClientName}, Trainer: {TrainerName}, Reason: {Reason}, SubscriptionID: {SubscriptionId}",
                    clientName, trainerName, reason, subscriptionId);

                // Raise event for subscribers
                if (OnSubscriptionCancelledAsync != null)
                {
                    await OnSubscriptionCancelledAsync(clientName, trainerName, reason, subscriptionId);
                }
                else
                {
                    _logger.LogWarning("No subscribers for OnSubscriptionCancelledAsync event");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing subscription cancelled notification");
                // Don't rethrow
            }
        }

        /// <summary>
        /// Publish subscription expiring soon notification
        /// </summary>
        public async Task NotifySubscriptionExpiringAsync(string clientName, string trainerName, DateTime expirationDate, string subscriptionId)
        {
            try
            {
                _logger.LogInformation(
                    "📢 Publishing admin notification: Subscription Expiring | Client: {ClientName}, Trainer: {TrainerName}, Expires: {ExpirationDate}, SubscriptionID: {SubscriptionId}",
                    clientName, trainerName, expirationDate, subscriptionId);

                // Raise event for subscribers
                if (OnSubscriptionExpiringAsync != null)
                {
                    await OnSubscriptionExpiringAsync(clientName, trainerName, expirationDate, subscriptionId);
                }
                else
                {
                    _logger.LogWarning("No subscribers for OnSubscriptionExpiringAsync event");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing subscription expiring notification");
                // Don't rethrow
            }
        }
    }
}
