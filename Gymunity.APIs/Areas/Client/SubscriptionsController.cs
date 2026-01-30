using Gymunity.APIs.Services;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.User.Subscriptions;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Areas.Client
{
    /// <summary>
    /// Controller for managing client package subscriptions, including creation, cancellation, and activation.
    /// </summary>
    /// <param name="subscriptionService">Service for subscription business logic.</param>
    /// <param name="adminNotificationService">Service for notifying administrators of subscription changes.</param>
    /// <param name="adminUserResolver">Service to identify admin users for notifications.</param>
    /// <param name="logger">Logger instance for tracking subscription events.</param>
    public class SubscriptionsController(
        ISubscriptionService subscriptionService,
        IAdminNotificationService adminNotificationService,
        AdminUserResolverService adminUserResolver,
        ILogger<SubscriptionsController> logger) : ClientBaseController
    {
        private readonly ISubscriptionService _subscriptionService = subscriptionService;
        private readonly IAdminNotificationService _adminNotificationService = adminNotificationService;
        private readonly AdminUserResolverService _adminUserResolver = adminUserResolver;
        private readonly ILogger<SubscriptionsController> _logger = logger;

        /// <summary>
        /// Determines whether the current user has an active subscription to the specified package.
        /// </summary>
        /// <param name="packageId">The identifier of the package to check for an active subscription.</param>
        /// <returns>An <see cref="IActionResult"/> containing a boolean value: <see langword="true"/> if the user has an active
        /// subscription to the package; otherwise, <see langword="false"/>.</returns>
        [HttpGet("HasActiveSubscription/{packageId}")]
        public async Task<IActionResult> HasActiveSubscription(int packageId)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "b8f4c7e9-1c1f-4c5c-a12d-9a8f12345678";
            var result = await _subscriptionService
                .HasActiveSubscribtionToPackageAsync(clientId, packageId);
            return Ok(result);
        }

        /// <summary>
        /// Subscribe to a package (creates UNPAID subscription)
        /// POST: api/client/subscriptions/subscribe
        /// </summary>
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe(
            [FromBody] SubscribePackageRequest request)
        {
            // 🔹 مؤقت لحد auth - هيبقى من الـ JWT Token
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "b8f4c7e9-1c1f-4c5c-a12d-9a8f12345678";

            var result = await _subscriptionService
                .SubscribeAsync(clientId, request);

            if (!result.IsSuccess)
                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage
                });

            // ✅ Notify admin of new subscription
            await NotifyAdminOfSubscriptionAsync(result.Data, true);

            return Ok(new
            {
                success = true,
                message = "Subscription created successfully. Please proceed to payment.",
                data = result.Data
            });
        }

        /// <summary>
        /// Get all client subscriptions
        /// GET: api/client/subscriptions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMySubscriptions(
            [FromQuery] SubscriptionStatus? status = null)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "b8f4c7e9-1c1f-4c5c-a12d-9a8f12345678";

            var result = await _subscriptionService
                .GetClientSubscriptionsAsync(clientId, status);

            if (!result.IsSuccess)
                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage
                });

            return Ok(new
            {
                success = true,
                data = result.Data
            });
        }

        /// <summary>
        /// Get single subscription details
        /// GET: api/client/subscriptions/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubscriptionById(int id)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "b8f4c7e9-1c1f-4c5c-a12d-9a8f12345678";

            var result = await _subscriptionService
                .GetSubscriptionByIdAsync(id, clientId);

            if (!result.IsSuccess)
                return NotFound(new
                {
                    success = false,
                    message = result.ErrorMessage
                });

            return Ok(new
            {
                success = true,
                data = result.Data
            });
        }

        /// <summary>
        /// Cancel subscription
        /// DELETE: api/client/subscriptions/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelSubscription(int id)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "b8f4c7e9-1c1f-4c5c-a12d-9a8f12345678";

            var result = await _subscriptionService
                .CancelSubscriptionAsync(id, clientId);

            if (!result.IsSuccess)
                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage
                });

            // ✅ Notify admin of subscription cancellation
            await NotifyAdminOfSubscriptionCancellationAsync(id);

            return Ok(new
            {
                success = true,
                message = "Subscription canceled successfully"
            });
        }

        /// <summary>
        /// Activate subscription after payment (called by payment webhook)
        /// POST: api/client/subscriptions/{id}/activate
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateSubscription(
            int id,
            [FromBody] ActivateSubscriptionRequest request)
        {
            var result = await _subscriptionService
                .ActivateSubscriptionAsync(id, request.TransactionId);

            if (!result.IsSuccess)
                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage
                });

            // ✅ Notify admin of subscription activation
            await NotifyAdminOfSubscriptionActivationAsync(result.Data);

            return Ok(new
            {
                success = true,
                message = "Subscription activated successfully",
                data = result.Data
            });
        }

        /// <summary>
        /// Sends admin notification for new subscription
        /// </summary>
        private async Task NotifyAdminOfSubscriptionAsync(object subscription, bool isCreation)
        {
            try
            {
                if (subscription == null)
                    return;

                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about subscription");
                    return;
                }

                await _adminNotificationService.CreateAdminNotificationAsync(
                    adminUserId: admin.Id,
                    title: "New Subscription Created",
                    message: "A new subscription has been created and is pending payment",
                    type: NotificationType.NewSubscription,
                    relatedEntityId: subscription.GetType().GetProperty("Id")?.GetValue(subscription)?.ToString(),
                    broadcastToAll: true
                );

                _logger.LogInformation("Admin notified of new subscription");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify admin of subscription");
                // Don't rethrow - subscription operation already succeeded
            }
        }

        /// <summary>
        /// Sends admin notification for subscription cancellation
        /// </summary>
        private async Task NotifyAdminOfSubscriptionCancellationAsync(int subscriptionId)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about subscription cancellation");
                    return;
                }

                await _adminNotificationService.CreateAdminNotificationAsync(
                    adminUserId: admin.Id,
                    title: "Subscription Cancelled",
                    message: $"Subscription {subscriptionId} has been cancelled",
                    type: NotificationType.SubscriptionCancelled,
                    relatedEntityId: subscriptionId.ToString(),
                    broadcastToAll: true
                );

                _logger.LogInformation("Admin notified of subscription cancellation {SubscriptionId}", subscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify admin of subscription cancellation");
                // Don't rethrow - cancellation operation already succeeded
            }
        }


        /// <summary>
        /// Sends admin notification for subscription activation
        /// </summary>
        private async Task NotifyAdminOfSubscriptionActivationAsync(object subscription)
        {
            try
            {
                if (subscription == null)
                    return;

                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about subscription activation");
                    return;
                }

                await _adminNotificationService.CreateAdminNotificationAsync(
                    adminUserId: admin.Id,
                    title: "Subscription Activated",
                    message: "A subscription has been activated after successful payment",
                    type: NotificationType.NewSubscription,
                    relatedEntityId: subscription.GetType().GetProperty("Id")?.GetValue(subscription)?.ToString(),
                    broadcastToAll: true
                );

                _logger.LogInformation("Admin notified of subscription activation");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify admin of subscription activation");
                // Don't rethrow - activation operation already succeeded
            }
        }
    }

    /// <summary>
    /// Request DTO for activating a subscription after a successful payment transaction.
    /// </summary>
    public class ActivateSubscriptionRequest
    {
        /// <summary>
        /// The unique transaction identifier provided by the payment gateway.
        /// </summary>
        public string TransactionId { get; set; } = null!;
    }
}