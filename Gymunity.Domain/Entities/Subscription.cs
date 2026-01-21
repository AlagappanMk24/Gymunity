using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;

namespace Gymunity.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public string ClientId { get; set; } = null!;
        public int PackageId { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Unpaid;  //Enum
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime CurrentPeriodEnd { get; set; }

        // Paymob
        public string? PaymobOrderId { get; set; }
        public string? PaymobTransactionId { get; set; }

        // PayPal
        public string? PayPalSubscriptionId { get; set; }
        public string? PayPalOrderId { get; set; }        // ✅ NEW: Order ID
        public string? PayPalApprovalUrl { get; set; }    // ✅ NEW: Approval URL
        public string? PayPalCaptureId { get; set; }      // ✅ NEW: Capture ID from webhook

        // Stripe
        public string? StripePaymentIntentId { get; set; }
        public string? StripeCheckoutUrl { get; set; }    // ✅ NEW: Checkout URL

        public string Currency { get; set; } = "USD";
        public decimal AmountPaid { get; set; }
        public decimal PlatformFeePercentage { get; set; } = 15m;

        public bool IsAnnual { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CanceledAt { get; set; }

        public AppUser Client { get; set; } = null!;
        public Package Package { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = [];
    }
}
