namespace Gymunity.Application.Contracts.Services
{
    /// <summary>
    /// Event-driven interface for publishing admin notifications
    /// Decouples Infrastructure layer from MVC admin layer
    /// Allows Infrastructure to trigger notifications without direct MVC references
    /// </summary>
    public interface IAdminNotificationPublisher
    {
        /// <summary>
        /// Notify admins of new payment received
        /// </summary>
        /// <param name="amount">Payment amount</param>
        /// <param name="clientName">Client name</param>
        /// <param name="paymentId">Payment ID</param>
        Task NotifyNewPaymentAsync(decimal amount, string clientName, string paymentId);

        /// <summary>
        /// Notify admins of payment failure
        /// </summary>
        /// <param name="amount">Payment amount</param>
        /// <param name="clientName">Client name</param>
        /// <param name="failureReason">Reason for failure</param>
        /// <param name="paymentId">Payment ID</param>
        Task NotifyPaymentFailureAsync(decimal amount, string clientName, string failureReason, string paymentId);

        /// <summary>
        /// Notify admins of new subscription
        /// </summary>
        /// <param name="clientName">Client name</param>
        /// <param name="trainerName">Trainer name</param>
        /// <param name="packageName">Package name</param>
        /// <param name="subscriptionId">Subscription ID</param>
        Task NotifyNewSubscriptionAsync(string clientName, string trainerName, string packageName, string subscriptionId);

        /// <summary>
        /// Notify admins of subscription cancellation
        /// </summary>
        /// <param name="clientName">Client name</param>
        /// <param name="trainerName">Trainer name</param>
        /// <param name="reason">Cancellation reason</param>
        /// <param name="subscriptionId">Subscription ID</param>
        Task NotifySubscriptionCancelledAsync(string clientName, string trainerName, string reason, string subscriptionId);

        /// <summary>
        /// Notify admins of subscription expiring soon
        /// </summary>
        /// <param name="clientName">Client name</param>
        /// <param name="trainerName">Trainer name</param>
        /// <param name="expirationDate">Subscription expiration date</param>
        /// <param name="subscriptionId">Subscription ID</param>
        Task NotifySubscriptionExpiringAsync(string clientName, string trainerName, DateTime expirationDate, string subscriptionId);
    }
}
