namespace Gymunity.Domain.Enums
{
    public enum NotificationType
    {
        NewMessage = 1,
        MessageReply = 2,
        SubscriptionConfirmed = 3,
        SubscriptionExpired = 4,
        PackageUpdate = 5,
        ProgramUpdate = 6,
        WorkoutReminder = 7,
        PaymentSuccessful = 8,
        PaymentFailed = 9,
        TrainerApproved = 10,
        TrainerRejected = 11,
        SystemNotification = 12,
        
        // Admin Notifications - New Registrations
        NewClientRegistration = 101,
        NewTrainerRegistration = 102,
        NewGuestRegistration = 103,
        
        // Admin Notifications - Subscriptions
        NewSubscription = 201,
        SubscriptionCancelled = 202,
        SubscriptionExpiring = 204,
        
        // Admin Notifications - Payments
        NewPayment = 301,
        PaymentFailure = 302,
        RefundProcessed = 303,
        PaymentReconciliation = 304,
        
        // Admin Notifications - Profile Actions
        TrainerProfileCreated = 401,
        TrainerProfileUpdated = 402,
        ClientProfileCreated = 403,
        ClientProfileUpdated = 404,
        ProfileDeleteRequested = 405,
        
        // Admin Notifications - Verification Required
        TrainerVerificationRequired = 501,
        ReviewCreated = 502,
        ReviewFlaggedForModeration = 503,
        ContentViolationDetected = 504,
        
        // Admin Notifications - Deletion Actions
        UserAccountDeleted = 601,
        TrainerProfileDeleted = 602,
        SubscriptionDeleted = 603,
        ReviewDeleted = 604,
        
        // Admin Notifications - Account Management
        AccountSuspended = 701,
        AccountReactivated = 702,
        UnusualActivityDetected = 703,
        SecurityIssueDetected = 704,
    }
}
