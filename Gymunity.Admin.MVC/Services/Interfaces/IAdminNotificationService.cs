using Gymunity.Application.DTOs.Notifications;
using Gymunity.Domain.Enums;

namespace Gymunity.Admin.MVC.Services.Interfaces
{
    /// <summary>
    /// Interface for admin notification management
    /// </summary>
    public interface IAdminNotificationService
    {
        // Core notification operations
        Task<NotificationResponse> CreateAdminNotificationAsync(
            string adminUserId,
            string title,
            string message,
            NotificationType type,
            string? relatedEntityId = null,
            bool broadcastToAll = false);

        Task<IEnumerable<NotificationResponse>> GetAdminUnreadNotificationsAsync(string adminUserId);
        Task<IEnumerable<NotificationResponse>> GetAdminNotificationsAsync(string adminUserId, int pageNumber = 1, int pageSize = 20);
        Task MarkNotificationAsReadAsync(int notificationId, string adminUserId);
        Task<int> GetUnreadNotificationCountAsync(string adminUserId);

        // Specific notification methods
        Task NotifyNewClientRegistrationAsync(string adminUserId, string clientName, string clientEmail, string clientId);
        Task NotifyNewTrainerRegistrationAsync(string adminUserId, string trainerName, string trainerEmail, string trainerId);
        Task NotifyNewSubscriptionAsync(string adminUserId, string clientName, string trainerName, string subscriptionId);
        Task NotifySubscriptionCancelledAsync(string adminUserId, string clientName, string trainerName, string subscriptionId);
        Task NotifyNewPaymentAsync(string adminUserId, decimal amount, string clientName, string paymentId);
        Task NotifyPaymentFailureAsync(string adminUserId, decimal amount, string clientName, string paymentId);
        Task NotifyTrainerVerificationRequiredAsync(string adminUserId, string trainerName, string trainerId);
        Task NotifyReviewCreatedAsync(string adminUserId, string reviewerName, string reviewerType, string reviewId);
        Task NotifyReviewFlaggedAsync(string adminUserId, string reviewReason, string reviewId);
        Task NotifyTrainerProfileCreatedAsync(string adminUserId, string trainerName, string trainerId);
        Task NotifyAccountSuspendedAsync(string adminUserId, string userName, string accountType, string userId);
        Task NotifyAccountReactivatedAsync(string adminUserId, string userName, string accountType, string userId);
        Task NotifyContentDeletedAsync(string adminUserId, string contentType, string contentName, string contentId);
        Task NotifyUnusualActivityAsync(string adminUserId, string userName, string activityDescription, string? relatedEntityId = null);
        Task NotifySecurityIssueAsync(string adminUserId, string issueDescription, string severity = "warning");
    }
}