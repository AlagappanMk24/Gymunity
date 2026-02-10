using Gymunity.Admin.MVC.Services.Interfaces;
using Gymunity.Application.Contracts.Services;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.Services.Admin;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Gymunity.Infrastructure.ExternalServices;

namespace Gymunity.Admin.MVC.Services
{
    /// <summary>
    /// Handles payment-related notifications
    /// Subscribes to events from:
    /// 1. PaymentAdminService (payment management operations)
    /// 2. AdminNotificationPublisher (webhook payment events)
    /// Sends notifications to admins via both local events and infrastructure events
    /// </summary>
    public class PaymentNotificationService : IPaymentNotificationService
    {
        private readonly IAdminNotificationService _notificationService;
        private readonly IPaymentAdminService _paymentAdminService;
        private readonly IAdminUserResolverService _adminUserResolver;
        private readonly ILogger<PaymentNotificationService> _logger;

        public PaymentNotificationService(
            IAdminNotificationService notificationService,
            IPaymentAdminService paymentAdminService,
            IAdminUserResolverService adminUserResolver,
            IAdminNotificationPublisher notificationPublisher,
            ILogger<PaymentNotificationService> logger)
        {
            _notificationService = notificationService;
            _paymentAdminService = paymentAdminService;
            _adminUserResolver = adminUserResolver;
            _logger = logger;

            // ✅ Subscribe to local PaymentAdminService events
            _paymentAdminService.RefundCompletedAsync += OnRefundCompletedAsync;
            _paymentAdminService.PaymentMarkedAsFailedAsync += OnPaymentMarkedAsFailedAsync;

            // ✅ Subscribe to webhook payment events from AdminNotificationPublisher
            // Cast to concrete implementation to access public events
            if (notificationPublisher is AdminNotificationPublisher publisher)
            {
                publisher.OnNewPaymentAsync += OnWebhookNewPaymentAsync;
                publisher.OnPaymentFailureAsync += OnWebhookPaymentFailureAsync;
                _logger.LogInformation("✅ PaymentNotificationService subscribed to webhook payment events");
            }
            else
            {
                _logger.LogWarning("⚠️ AdminNotificationPublisher is not the expected concrete type");
            }

            _logger.LogInformation("✅ PaymentNotificationService subscribed to local payment events");
        }

        /// <summary>
        /// Handle refund completion event (local admin operation)
        /// Creates and broadcasts notification to all admins
        /// </summary>
        private async Task OnRefundCompletedAsync(int paymentId, Payment payment)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about refund {PaymentId}", paymentId);
                    return;
                }

                // Client is stored directly as AppUser in Subscription
                var clientName = payment.Subscription?.Client?.FullName ?? "Unknown Client";
                
                await _notificationService.CreateAdminNotificationAsync(
                    admin.Id,
                    "Payment Refunded",
                    $"Refund of {payment.Amount:C} processed for {clientName}",
                    NotificationType.RefundProcessed,
                    payment.Id.ToString(),
                    broadcastToAll: true);

                _logger.LogInformation("Admin notified of refund for payment {PaymentId}", paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send refund notification for payment {PaymentId}", paymentId);
                // Non-blocking - don't rethrow
            }
        }

        /// <summary>
        /// Handle payment failure event (local admin operation)
        /// Creates and broadcasts notification to all admins
        /// </summary>
        private async Task OnPaymentMarkedAsFailedAsync(int paymentId, Payment payment)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about payment failure {PaymentId}", paymentId);
                    return;
                }

                // Client is stored directly as AppUser in Subscription
                var clientName = payment.Subscription?.Client?.FullName ?? "Unknown Client";
                
                await _notificationService.CreateAdminNotificationAsync(
                    admin.Id,
                    "Payment Failed",
                    $"Payment of {payment.Amount:C} from {clientName} has failed",
                    NotificationType.PaymentFailure,
                    payment.Id.ToString(),
                    broadcastToAll: false);

                _logger.LogInformation("Admin notified of payment failure for payment {PaymentId}", paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send payment failure notification for payment {PaymentId}", paymentId);
                // Non-blocking - don't rethrow
            }
        }

        /// <summary>
        /// Handle new payment event from webhook (Infrastructure layer)
        /// </summary>
        private async Task OnWebhookNewPaymentAsync(decimal amount, string clientName, string paymentId)
        {
            try
            {
                _logger.LogInformation(
                    "📬 Handling webhook new payment event | Amount: {Amount}, Client: {ClientName}, PaymentID: {PaymentId}",
                    amount, clientName, paymentId);

                // Get all admins
                var admins = await _adminUserResolver.GetAllAdminsAsync();

                // Send notification to all admins
                foreach (var admin in admins)
                {
                    try
                    {
                        await _notificationService.NotifyNewPaymentAsync(
                            admin.Id,
                            amount,
                            clientName,
                            paymentId);

                        _logger.LogInformation(
                            "✅ New payment notification sent to admin {AdminId} | Client: {ClientName}",
                            admin.Id, clientName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to send new payment notification to admin {AdminId}",
                            admin.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error handling webhook new payment event | Amount: {Amount}, Client: {ClientName}",
                    amount, clientName);
            }
        }

        /// <summary>
        /// Handle payment failure event from webhook (Infrastructure layer)
        /// </summary>
        private async Task OnWebhookPaymentFailureAsync(decimal amount, string clientName, string failureReason, string paymentId)
        {
            try
            {
                _logger.LogWarning(
                    "📬 Handling webhook payment failure event | Amount: {Amount}, Client: {ClientName}, Reason: {Reason}, PaymentID: {PaymentId}",
                    amount, clientName, failureReason, paymentId);

                // Get all admins
                var admins = await _adminUserResolver.GetAllAdminsAsync();

                // Send notification to all admins
                foreach (var admin in admins)
                {
                    try
                    {
                        await _notificationService.NotifyPaymentFailureAsync(
                            admin.Id,
                            amount,
                            clientName,
                            paymentId);

                        _logger.LogInformation(
                            "✅ Payment failure notification sent to admin {AdminId} | Client: {ClientName}",
                            admin.Id, clientName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to send payment failure notification to admin {AdminId}",
                            admin.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error handling webhook payment failure event | Amount: {Amount}, Client: {ClientName}, Reason: {Reason}",
                    amount, clientName, failureReason);
            }
        }
    }
}
