

using Gymunity.Domain.Enums;

namespace Gymunity.Domain.Entities
{
    public class Payment : BaseEntity
    {
        // Relations
        public int SubscriptionId { get; set; }
        public string ClientId { get; set; } = null!; //  Added for better queries

        // Amount Details
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal PlatformFee { get; set; } = 0;// 15% of Amount
        public decimal TrainerPayout { get; set; } = 0; // Amount - PlatformFee

        // Status & Method
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentMethod Method { get; set; }

        // Paymob Integration
        public string? PaymobOrderId { get; set; }
        public string? PaymobTransactionId { get; set; }
        public int? PaymobIntegrationId { get; set; }

        // PayPal Integration
        public string? PayPalOrderId { get; set; }
        public string? PayPalPayerId { get; set; }
        public string? PayPalCaptureId { get; set; }

        // Stripe Integration
        public string? StripeSessionId { get; set; }

        // Metadata
        public string? PaymentMethodType { get; set; } // card, wallet, etc
        public string? FailureReason { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }


        // Dates
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime? RefundedAt { get; set; }

        // Navigation
        public Subscription Subscription { get; set; } = null!;

        //  Helper method to calculate fees
        public void CalculateFees(decimal platformFeePercentage = 15m)
        {
            PlatformFee = Amount * (platformFeePercentage / 100m);
            TrainerPayout = Amount - PlatformFee;
        }
    }
}