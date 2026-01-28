namespace Gymunity.Admin.MVC.ViewModels.Notifications
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }

        // UI Helper Properties
        public string NotificationIcon => GetIconForType(Type);
        public string NotificationColor => GetColorForType(Type);
        public string NotificationCategory => GetCategoryForType(Type);
        public string FormattedTime => FormatTime(CreatedAt);
        public bool IsHighPriority => IsHighPriorityType(Type);

        private string GetIconForType(string type) => type switch
        {
            "NewClientRegistration" => "fas fa-user-plus",
            "NewTrainerRegistration" => "fas fa-user-tie",
            "NewGuestRegistration" => "fas fa-user-check",
            "NewSubscription" => "fas fa-handshake",
            "SubscriptionCancelled" => "fas fa-ban",
            "SubscriptionExpiring" => "fas fa-hourglass-end",
            "NewPayment" => "fas fa-credit-card",
            "PaymentFailure" => "fas fa-exclamation-circle",
            "RefundProcessed" => "fas fa-undo",
            "PaymentReconciliation" => "fas fa-receipt",
            "TrainerProfileCreated" => "fas fa-id-card",
            "TrainerProfileUpdated" => "fas fa-edit",
            "ClientProfileCreated" => "fas fa-id-badge",
            "ClientProfileUpdated" => "fas fa-edit",
            "ProfileDeleteRequested" => "fas fa-trash",
            "TrainerVerificationRequired" => "fas fa-check-circle",
            "ReviewCreated" => "fas fa-comments",
            "ReviewFlaggedForModeration" => "fas fa-flag",
            "ContentViolationDetected" => "fas fa-exclamation-triangle",
            "UserAccountDeleted" => "fas fa-user-slash",
            "TrainerProfileDeleted" => "fas fa-trash-alt",
            "SubscriptionDeleted" => "fas fa-trash-alt",
            "ReviewDeleted" => "fas fa-trash-alt",
            "AccountSuspended" => "fas fa-lock",
            "AccountReactivated" => "fas fa-unlock",
            "UnusualActivityDetected" => "fas fa-exclamation-diamond",
            "SecurityIssueDetected" => "fas fa-shield-exclamation",
            _ => "fas fa-bell"
        };

        private string GetColorForType(string type) => type switch
        {
            "NewClientRegistration" => "blue",
            "NewTrainerRegistration" => "purple",
            "NewGuestRegistration" => "cyan",
            "NewSubscription" => "green",
            "SubscriptionCancelled" => "red",
            "SubscriptionExpiring" => "yellow",
            "NewPayment" => "emerald",
            "PaymentFailure" => "red",
            "RefundProcessed" => "amber",
            "PaymentReconciliation" => "indigo",
            "TrainerProfileCreated" => "teal",
            "TrainerProfileUpdated" => "blue",
            "ClientProfileCreated" => "cyan",
            "ClientProfileUpdated" => "blue",
            "ProfileDeleteRequested" => "orange",
            "TrainerVerificationRequired" => "purple",
            "ReviewCreated" => "blue",
            "ReviewFlaggedForModeration" => "orange",
            "ContentViolationDetected" => "red",
            "UserAccountDeleted" => "red",
            "TrainerProfileDeleted" => "red",
            "SubscriptionDeleted" => "red",
            "ReviewDeleted" => "red",
            "AccountSuspended" => "red",
            "AccountReactivated" => "green",
            "UnusualActivityDetected" => "orange",
            "SecurityIssueDetected" => "red",
            _ => "gray"
        };

        private string GetCategoryForType(string type) => type switch
        {
            "NewClientRegistration" or "NewTrainerRegistration" or "NewGuestRegistration" => "Registrations",
            "NewSubscription" or "SubscriptionCancelled" or "SubscriptionExpiring" or "SubscriptionDeleted" => "Subscriptions",
            "NewPayment" or "PaymentFailure" or "RefundProcessed" or "PaymentReconciliation" => "Payments",
            "TrainerProfileCreated" or "TrainerProfileUpdated" or "ClientProfileCreated" or "ClientProfileUpdated" 
                or "ProfileDeleteRequested" or "TrainerProfileDeleted" => "Profiles",
            "TrainerVerificationRequired" or "ReviewCreated" or "ReviewFlaggedForModeration" or "ReviewDeleted" or "ContentViolationDetected" => "Moderation",
            "UserAccountDeleted" or "AccountSuspended" or "AccountReactivated" => "Account Management",
            "UnusualActivityDetected" or "SecurityIssueDetected" => "Security",
            _ => "System"
        };

        private bool IsHighPriorityType(string type) => type switch
        {
            "PaymentFailure" or "AccountSuspended" or "SecurityIssueDetected" or "UnusualActivityDetected" 
                or "ReviewFlaggedForModeration" or "ContentViolationDetected" or "SubscriptionCancelled" => true,
            _ => false
        };

        private string FormatTime(DateTime dateTime)
        {
            var now = DateTime.UtcNow;
            var diff = now - dateTime;

            return diff.TotalSeconds < 60 ? "Just now"
                : diff.TotalMinutes < 60 ? $"{(int)diff.TotalMinutes}m ago"
                : diff.TotalHours < 24 ? $"{(int)diff.TotalHours}h ago"
                : diff.TotalDays < 7 ? $"{(int)diff.TotalDays}d ago"
                : dateTime.ToString("MMM dd, yyyy");
        }
    }
}
