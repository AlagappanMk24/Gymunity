using Gymunity.Domain.Entities;

namespace Gymunity.Application.Contracts.ExternalServices
{
    public interface IStripePaymentService
    {
        /// <summary>
        /// Creates a Stripe Checkout Session for subscription payment
        /// Returns a URL to Stripe's hosted checkout page (similar to PayPal's approval URL)
        /// </summary>
        Task<(bool Success, string? CheckoutUrl, string? SessionId, string? ErrorMessage)>
            CreateCheckoutSessionAsync(Subscription subscription, string returnUrl, string cancelUrl);

        /// <summary>
        /// Retrieves a Checkout Session by ID
        /// </summary>
        Task<(bool Success, string? Status, string? PaymentIntentId, string? ErrorMessage)>
            GetCheckoutSessionAsync(string sessionId);

        /// <summary>
        /// Verifies webhook signature from Stripe
        /// </summary>
        bool VerifyWebhookSignature(string payload, string signature);

        /// <summary>
        /// Refunds a Stripe payment
        /// </summary>
        Task<(bool Success, string? RefundId, string? ErrorMessage)>
            RefundPaymentAsync(string paymentIntentId, decimal? amount = null);
    }
}