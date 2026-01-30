using Gymunity.Application.DTOs.Notifications;
using Gymunity.Domain.Enums;

namespace Gymunity.APIs.Services
{
    /// <summary>
    /// Interface for admin notification management
    /// </summary>
    public interface IAdminNotificationService
    {
        /// <summary>
        /// Creates a general notification for an administrator.
        /// </summary>
        /// <param name="adminUserId">The ID of the admin user.</param>
        /// <param name="title">The title of the notification.</param>
        /// <param name="message">The main content of the notification.</param>
        /// <param name="type">The category/type of the notification.</param>
        /// <param name="relatedEntityId">Optional ID of the entity (Client, Trainer, etc.) related to this alert.</param>
        /// <param name="broadcastToAll">If true, sends the notification to all administrators.</param>
        /// <returns>A response object containing notification details.</returns>
        Task<NotificationResponse> CreateAdminNotificationAsync(
            string adminUserId,
            string title,
            string message,
            NotificationType type,
            string? relatedEntityId = null,
            bool broadcastToAll = false);

        /// <summary>Retrieves unread notifications for a specific admin.</summary>
        Task<IEnumerable<NotificationResponse>> GetAdminUnreadNotificationsAsync(string adminUserId);

        /// <summary>Retrieves a paginated list of all notifications for an admin.</summary>
        Task<IEnumerable<NotificationResponse>> GetAdminNotificationsAsync(string adminUserId, int pageNumber = 1, int pageSize = 20);

        /// <summary>Marks a specific notification as read by an admin.</summary>
        Task MarkNotificationAsReadAsync(int notificationId, string adminUserId);

        /// <summary>Gets the count of unread notifications for a specific admin.</summary>
        Task<int> GetUnreadNotificationCountAsync(string adminUserId);

        /// <summary>Notifies admin when a new client registers.</summary>
        Task NotifyNewClientRegistrationAsync(string adminUserId, string clientName, string clientEmail, string clientId);
        /// <summary>Notifies admin when a new trainer registers.</summary>
        Task NotifyNewTrainerRegistrationAsync(string adminUserId, string trainerName, string trainerEmail, string trainerId);

        /// <summary>Notifies admin of a new subscription purchase.</summary>
        Task NotifyNewSubscriptionAsync(string adminUserId, string clientName, string trainerName, string subscriptionId);

        /// <summary>Notifies admin when a subscription is cancelled.</summary>
        Task NotifySubscriptionCancelledAsync(string adminUserId, string clientName, string trainerName, string subscriptionId);

        /// <summary>Notifies admin of a successful payment.</summary>
        Task NotifyNewPaymentAsync(string adminUserId, decimal amount, string clientName, string paymentId);

        /// <summary>Notifies admin when a payment attempt fails.</summary>
        Task NotifyPaymentFailureAsync(string adminUserId, decimal amount, string clientName, string paymentId);

        /// <summary>Notifies admin that a trainer's documents/profile require verification.</summary>
        Task NotifyTrainerVerificationRequiredAsync(string adminUserId, string trainerName, string trainerId);

        /// <summary>Notifies admin when a user submits a review.</summary>
        Task NotifyReviewCreatedAsync(string adminUserId, string reviewerName, string reviewerType, string reviewId);

        /// <summary>Notifies admin when a review has been flagged for moderation.</summary>
        Task NotifyReviewFlaggedAsync(string adminUserId, string reviewReason, string reviewId);

        /// <summary>Notifies admin when a trainer successfully creates their profile.</summary>
        Task NotifyTrainerProfileCreatedAsync(string adminUserId, string trainerName, string trainerId);

        /// <summary>Notifies admin when an account has been suspended.</summary>
        Task NotifyAccountSuspendedAsync(string adminUserId, string userName, string accountType, string userId);

        /// <summary>Notifies admin when a suspended account is reactivated.</summary>
        Task NotifyAccountReactivatedAsync(string adminUserId, string userName, string accountType, string userId);

        /// <summary>Notifies admin of content deletion within the system.</summary>
        Task NotifyContentDeletedAsync(string adminUserId, string contentType, string contentName, string contentId);

        /// <summary>Notifies admin of unusual user behavior or access patterns.</summary>
        Task NotifyUnusualActivityAsync(string adminUserId, string userName, string activityDescription, string? relatedEntityId = null);

        /// <summary>Notifies admin of potential security risks or vulnerabilities.</summary>
        Task NotifySecurityIssueAsync(string adminUserId, string issueDescription, string severity = "warning");
    }
}