using Gymunity.APIs.Hubs;
using Gymunity.Application.Contracts.Services.Communication;
using Gymunity.Application.DTOs.Notifications;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Gymunity.APIs.Services
{
    /// <summary>
    /// Service for managing admin notifications
    /// Handles creation, retrieval, and real-time notification delivery to admin users
    /// </summary>
    public class AdminNotificationService(
        IHubContext<AdminNotificationHub> hubContext,
        ILogger<AdminNotificationService> logger,
        INotificationService notificationService) : IAdminNotificationService
    {
        private readonly IHubContext<AdminNotificationHub> _hubContext = hubContext;
        private readonly ILogger<AdminNotificationService> _logger = logger;
        private readonly INotificationService _notificationService = notificationService;

        /// <summary>
        /// Create and broadcast admin notification
        /// </summary>
        public async Task<NotificationResponse> CreateAdminNotificationAsync(
            string adminUserId,
            string title,
            string message,
            NotificationType type,
            string? relatedEntityId = null,
            bool broadcastToAll = false)
        {
            try
            {
                // Create notification in database
                var notificationResponse = await _notificationService.CreateNotificationAsync(
                    adminUserId,
                    title,
                    message,
                    (int)type,
                    relatedEntityId);

                // Send real-time notification via SignalR
                if (broadcastToAll)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                    {
                        notificationResponse.Id,
                        Title = title,
                        Message = message,
                        Type = type.ToString(),
                        RelatedEntityId = relatedEntityId,
                        notificationResponse.CreatedAt,
                        IsRead = false
                    });
                }
                else
                {
                    await _hubContext.Clients.Group($"admin_{adminUserId}").SendAsync("ReceiveNotification", new
                    {
                        notificationResponse.Id,
                        Title = title,
                        Message = message,
                        Type = type.ToString(),
                        RelatedEntityId = relatedEntityId,
                        notificationResponse.CreatedAt,
                        IsRead = false
                    });
                }

                return notificationResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin notification for user {UserId}", adminUserId);
                throw;
            }
        }

        /// <summary>
        /// Notify admin of new client registration
        /// </summary>
        public async Task NotifyNewClientRegistrationAsync(string adminUserId, string clientName, string clientEmail, string clientId)
        {
            var title = "New Client Registration";
            var message = $"New client registered: {clientName} ({clientEmail})";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.NewClientRegistration, clientId, broadcastToAll: true);
            _logger.LogInformation("Admin notified of new client registration: {ClientName}", clientName);
        }

        /// <summary>
        /// Notify admin of new trainer registration
        /// </summary>
        public async Task NotifyNewTrainerRegistrationAsync(string adminUserId, string trainerName, string trainerEmail, string trainerId)
        {
            var title = "New Trainer Registration";
            var message = $"New trainer registered: {trainerName} ({trainerEmail})";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.NewTrainerRegistration, trainerId, broadcastToAll: true);
            _logger.LogInformation("Admin notified of new trainer registration: {TrainerName}", trainerName);
        }

        /// <summary>
        /// Notify admin of new subscription
        /// </summary>
        public async Task NotifyNewSubscriptionAsync(string adminUserId, string clientName, string trainerName, string subscriptionId)
        {
            var title = "New Subscription Created";
            var message = $"New subscription: {clientName} subscribed to {trainerName}";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.NewSubscription, subscriptionId, broadcastToAll: true);
            _logger.LogInformation("Admin notified of new subscription");
        }

        /// <summary>
        /// Notify admin of subscription cancellation
        /// </summary>
        public async Task NotifySubscriptionCancelledAsync(string adminUserId, string clientName, string trainerName, string subscriptionId)
        {
            var title = "Subscription Cancelled";
            var message = $"Subscription cancelled: {clientName} - {trainerName}";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.SubscriptionCancelled, subscriptionId);
            _logger.LogInformation("Admin notified of subscription cancellation");
        }

        /// <summary>
        /// Notify admin of new payment
        /// </summary>
        public async Task NotifyNewPaymentAsync(string adminUserId, decimal amount, string clientName, string paymentId)
        {
            var title = "New Payment Received";
            var message = $"New payment of {amount:C} from {clientName}";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.NewPayment, paymentId, broadcastToAll: true);
            _logger.LogInformation("Admin notified of new payment: {Amount}", amount);
        }

        /// <summary>
        /// Notify admin of payment failure
        /// </summary>
        public async Task NotifyPaymentFailureAsync(string adminUserId, decimal amount, string clientName, string paymentId)
        {
            var title = "Payment Failed";
            var message = $"Payment of {amount:C} from {clientName} failed";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.PaymentFailure, paymentId);
            _logger.LogInformation("Admin notified of payment failure");
        }

        /// <summary>
        /// Notify admin that trainer profile requires verification
        /// </summary>
        public async Task NotifyTrainerVerificationRequiredAsync(string adminUserId, string trainerName, string trainerId)
        {
            var title = "Trainer Verification Required";
            var message = $"Trainer profile pending verification: {trainerName}";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.TrainerVerificationRequired, trainerId, broadcastToAll: true);
            _logger.LogInformation("Admin notified of trainer verification requirement: {TrainerName}", trainerName);
        }

        /// <summary>
        /// Notify admin of review creation
        /// </summary>
        public async Task NotifyReviewCreatedAsync(string adminUserId, string reviewerName, string reviewerType, string reviewId)
        {
            var title = "New Review Created";
            var message = $"New {reviewerType} review from {reviewerName} - pending moderation";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.ReviewCreated, reviewId);
            _logger.LogInformation("Admin notified of new review creation");
        }

        /// <summary>
        /// Notify admin of flagged review for moderation
        /// </summary>
        public async Task NotifyReviewFlaggedAsync(string adminUserId, string reviewReason, string reviewId)
        {
            var title = "Review Flagged for Moderation";
            var message = $"Review flagged: {reviewReason}";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.ReviewFlaggedForModeration, reviewId);
            _logger.LogInformation("Admin notified of flagged review");
        }

        /// <summary>
        /// Notify admin of trainer profile creation
        /// </summary>
        public async Task NotifyTrainerProfileCreatedAsync(string adminUserId, string trainerName, string trainerId)
        {
            var title = "Trainer Profile Created";
            var message = $"New trainer profile created: {trainerName}";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.TrainerProfileCreated, trainerId, broadcastToAll: true);
            _logger.LogInformation("Admin notified of trainer profile creation");
        }

        /// <summary>
        /// Notify admin of account suspension
        /// </summary>
        public async Task NotifyAccountSuspendedAsync(string adminUserId, string userName, string accountType, string userId)
        {
            var title = "Account Suspended";
            var message = $"{accountType} account suspended: {userName}";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.AccountSuspended, userId, broadcastToAll: true);
            _logger.LogInformation("Admin notified of account suspension: {UserName}", userName);
        }

        /// <summary>
        /// Notify admin of account reactivation
        /// </summary>
        public async Task NotifyAccountReactivatedAsync(string adminUserId, string userName, string accountType, string userId)
        {
            var title = "Account Reactivated";
            var message = $"{accountType} account reactivated: {userName}";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.AccountReactivated, userId, broadcastToAll: true);
            _logger.LogInformation("Admin notified of account reactivation: {UserName}", userName);
        }

        /// <summary>
        /// Notify admin of content deletion
        /// </summary>
        public async Task NotifyContentDeletedAsync(string adminUserId, string contentType, string contentName, string contentId)
        {
            NotificationType type = contentType.ToLower() switch
            {
                "user" => NotificationType.UserAccountDeleted,
                "trainer" => NotificationType.TrainerProfileDeleted,
                "subscription" => NotificationType.SubscriptionDeleted,
                "review" => NotificationType.ReviewDeleted,
                _ => NotificationType.SystemNotification
            };

            var title = $"{contentType} Deleted";
            var message = $"{contentType} deleted: {contentName}";

            await CreateAdminNotificationAsync(adminUserId, title, message, type, contentId);
            _logger.LogInformation("Admin notified of {ContentType} deletion: {ContentName}", contentType, contentName);
        }

        /// <summary>
        /// Notify admin of unusual activity
        /// </summary>
        public async Task NotifyUnusualActivityAsync(string adminUserId, string userName, string activityDescription, string? relatedEntityId = null)
        {
            var title = "Unusual Activity Detected";
            var message = $"{activityDescription} - {userName}";

            await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.UnusualActivityDetected, relatedEntityId);
            _logger.LogWarning("Admin notified of unusual activity for user: {UserName}", userName);
        }

        /// <summary>
        /// Notify admin of security issue
        /// </summary>
        public async Task NotifySecurityIssueAsync(string adminUserId, string issueDescription, string severity = "warning")
        {
            var title = "Security Issue Detected";
            var message = issueDescription;

            var notification = await CreateAdminNotificationAsync(adminUserId, title, message, NotificationType.SecurityIssueDetected, broadcastToAll: true);

            // Send critical alert
            await _hubContext.Clients.All.SendAsync("ReceiveCriticalAlert", new
            {
                Title = title,
                Message = message,
                Severity = severity,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogError("Admin notified of security issue: {Issue}", issueDescription);
        }

        /// <summary>
        /// Get admin's unread notifications
        /// </summary>
        public async Task<IEnumerable<NotificationResponse>> GetAdminUnreadNotificationsAsync(string adminUserId)
        {
            try
            {
                return await _notificationService.GetUserNotificationsAsync(adminUserId, unreadOnly: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notifications for admin {UserId}", adminUserId);
                throw;
            }
        }

        /// <summary>
        /// Get all admin notifications
        /// </summary>
        public async Task<IEnumerable<NotificationResponse>> GetAdminNotificationsAsync(string adminUserId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var allNotifications = await _notificationService.GetUserNotificationsAsync(adminUserId, unreadOnly: false);
                return allNotifications.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for admin {UserId}", adminUserId);
                throw;
            }
        }

        /// <summary>
        /// Mark notification as read and update count via SignalR
        /// </summary>
        public async Task MarkNotificationAsReadAsync(int notificationId, string adminUserId)
        {
            try
            {
                await _notificationService.MarkNotificationAsReadAsync(notificationId);
                
                // Update unread count for user
                var unreadCount = await _notificationService.GetUnreadNotificationCountAsync(adminUserId);
                await _hubContext.Clients.Group($"admin_{adminUserId}").SendAsync("UpdateNotificationCount", unreadCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                throw;
            }
        }

        /// <summary>
        /// Get unread notification count for admin
        /// </summary>
        public async Task<int> GetUnreadNotificationCountAsync(string adminUserId)
        {
            try
            {
                return await _notificationService.GetUnreadNotificationCountAsync(adminUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for admin {UserId}", adminUserId);
                throw;
            }
        }
    }
}