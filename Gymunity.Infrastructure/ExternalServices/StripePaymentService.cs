using Gymunity.Application.Configuration;
using Gymunity.Application.Contracts.ExternalServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using DomainSubscription = Gymunity.Domain.Entities.Subscription;

namespace Gymunity.Infrastructure.ExternalServices
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly StripeSettings _settings;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(
            IOptions<StripeSettings> settings,
            ILogger<StripePaymentService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            // Initialize Stripe API key
            StripeConfiguration.ApiKey = _settings.SecretKey;
        }

        /// <summary>
        /// Creates a Stripe Checkout Session for subscription payment
        /// Returns a URL to Stripe's hosted checkout page (similar to PayPal approval URL)
        /// </summary>
        public async Task<(bool Success, string? CheckoutUrl, string? SessionId, string? ErrorMessage)>
            CreateCheckoutSessionAsync(DomainSubscription subscription, string returnUrl, string cancelUrl)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    Mode = "payment",
                    SuccessUrl = returnUrl,
                    CancelUrl = cancelUrl,
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(subscription.AmountPaid * 100), // Convert to cents
                                Currency = subscription.Currency.ToLower(),
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = subscription.Package?.Name ?? "Training Package",
                                    Description = $"Subscription Payment - Package: {subscription.PackageId}",
                                    Metadata = new Dictionary<string, string>
                                    {
                                        { "SubscriptionId", subscription.Id.ToString() },
                                        { "ClientId", subscription.ClientId },
                                        { "PackageId", subscription.PackageId.ToString() }
                                    }
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "SubscriptionId", subscription.Id.ToString() },
                        { "ClientId", subscription.ClientId },
                        { "PackageId", subscription.PackageId.ToString() }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Stripe Checkout Session created: {SessionId} for subscription {SubscriptionId}",
                    session.Id,
                    subscription.Id);

                return (true, session.Url, session.Id, null);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, 
                    "Stripe error creating checkout session for subscription {SubscriptionId}: {StripeErrorCode}",
                    subscription.Id,
                    ex.StripeError?.Code);

                return (false, null, null, ex.StripeError?.Message ?? ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe checkout session for subscription {SubscriptionId}", subscription.Id);
                return (false, null, null, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a Checkout Session by ID
        /// </summary>
        public async Task<(bool Success, string? Status, string? PaymentIntentId, string? ErrorMessage)>
            GetCheckoutSessionAsync(string sessionId)
        {
            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(sessionId);

                _logger.LogInformation(
                    "Retrieved Stripe Checkout Session {SessionId} with status {Status}",
                    sessionId,
                    session.PaymentStatus);

                return (true, session.PaymentStatus, session.PaymentIntentId, null);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex,
                    "Stripe error retrieving checkout session {SessionId}: {StripeErrorCode}",
                    sessionId,
                    ex.StripeError?.Code);

                return (false, null, null, ex.StripeError?.Message ?? ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Stripe checkout session {SessionId}", sessionId);
                return (false, null, null, ex.Message);
            }
        }

        /// <summary>
        /// Refunds a Stripe payment
        /// </summary>
        public async Task<(bool Success, string? RefundId, string? ErrorMessage)>
            RefundPaymentAsync(string paymentIntentId, decimal? amount = null)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId
                };

                if (amount.HasValue)
                {
                    options.Amount = (long)(amount.Value * 100); // Convert to cents
                }

                var service = new RefundService();
                var refund = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Stripe refund created: {RefundId} for payment intent {PaymentIntentId}",
                    refund.Id,
                    paymentIntentId);

                return (true, refund.Id, null);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex,
                    "Stripe error refunding payment intent {PaymentIntentId}: {StripeErrorCode}",
                    paymentIntentId,
                    ex.StripeError?.Code);

                return (false, null, ex.StripeError?.Message ?? ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding Stripe payment intent {PaymentIntentId}", paymentIntentId);
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Verifies webhook signature from Stripe
        /// </summary>
        public bool VerifyWebhookSignature(string payload, string signature)
        {
            try
            {
                EventUtility.ValidateSignature(payload, signature, _settings.WebhookSecret);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature verification failed");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Stripe webhook signature");
                return false;
            }
        }
    }
}