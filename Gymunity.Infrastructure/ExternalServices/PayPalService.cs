using Gymunity.Application.Configuration;
using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Gymunity.Infrastructure.ExternalServices
{
    public class PayPalService : IPayPalService
    {
        private readonly PayPalSettings _settings;
        private readonly ILogger<PayPalService> _logger;
        private readonly PayPalHttpClient _client;
        private readonly HttpClient _httpClient;

        public PayPalService(
            IOptions<PayPalSettings> settings,
            ILogger<PayPalService> logger,
            HttpClient httpClient)
        {
            _settings = settings.Value;
            _logger = logger;
            _httpClient = httpClient;

            // Initialize PayPal client
            PayPalEnvironment environment;

            if (_settings.Mode == "Live")
            {
                environment = new LiveEnvironment(_settings.ClientId, _settings.ClientSecret);
            }
            else
            {
                environment = new SandboxEnvironment(_settings.ClientId, _settings.ClientSecret);
            }

            _client = new PayPalHttpClient(environment);
        }

        /// <summary>
        /// Create PayPal order for subscription (new architecture - uses Subscription context)
        /// </summary>
        public async Task<(bool Success, string? OrderId, string? ApprovalUrl, string? ErrorMessage)>
            CreateOrderAsync(Subscription subscription, string returnUrl, string cancelUrl)
        {
            try
            {
                var orderRequest = new OrderRequest
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    PurchaseUnits =
                    [
                        new PurchaseUnitRequest
                        {
                            ReferenceId = subscription.Id.ToString(),
                            Description = $"Subscription Payment - Package: {subscription.PackageId}",
                            CustomId = subscription.Id.ToString(),
                            SoftDescriptor = "TrainerHub",
                            AmountWithBreakdown = new AmountWithBreakdown
                            {
                                CurrencyCode = subscription.Currency,
                                Value = subscription.AmountPaid.ToString("F2")
                            }
                        }
                    ],
                    ApplicationContext = new ApplicationContext
                    {
                        ReturnUrl = returnUrl,
                        CancelUrl = cancelUrl,
                        BrandName = "TrainerHub",
                        LandingPage = "BILLING",
                        UserAction = "PAY_NOW",
                        ShippingPreference = "NO_SHIPPING"
                    }
                };

                var request = new OrdersCreateRequest();
                request.Prefer("return=representation");
                request.RequestBody(orderRequest);

                var response = await _client.Execute(request);
                var result = response.Result<Order>();

                // Get approval URL
                var approvalUrl = result.Links
                    .FirstOrDefault(l => l.Rel == "approve")?.Href;

                _logger.LogInformation(
                    "PayPal order created: {OrderId} for subscription {SubscriptionId}",
                    result.Id,
                    subscription.Id);

                return (true, result.Id, approvalUrl, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal order for subscription {SubscriptionId}", subscription.Id);
                return (false, null, null, ex.Message);
            }
        }

        /// <summary>
        /// Capture PayPal order (after user approves)
        /// </summary>
        public async Task<(bool Success, string? CaptureId, string? ErrorMessage)>
            CaptureOrderAsync(string orderId)
        {
            try
            {
                var request = new OrdersCaptureRequest(orderId);
                request.Prefer("return=representation");
                request.RequestBody(new OrderActionRequest());

                var response = await _client.Execute(request);
                var result = response.Result<Order>();

                var captureId = result.PurchaseUnits[0]
                    .Payments.Captures[0].Id;

                _logger.LogInformation(
                    "PayPal order {OrderId} captured: {CaptureId}",
                    orderId,
                    captureId);

                return (true, captureId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing PayPal order {OrderId}", orderId);
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Get order details
        /// </summary>
        public async Task<dynamic?> GetOrderAsync(string orderId)
        {
            try
            {
                var request = new OrdersGetRequest(orderId);
                var response = await _client.Execute(request);
                return response.Result<Order>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayPal order {OrderId}", orderId);
                return null;
            }
        }

        /// <summary>
        /// Refund capture
        /// </summary>
        public async Task<(bool Success, string? RefundId, string? ErrorMessage)>
            RefundCaptureAsync(string captureId, decimal amount, string currency)
        {
            try
            {
                var request = new PayPalCheckoutSdk.Payments.CapturesRefundRequest(captureId);
                request.Prefer("return=representation");
                request.RequestBody(new PayPalCheckoutSdk.Payments.RefundRequest
                {
                    Amount = new PayPalCheckoutSdk.Payments.Money
                    {
                        CurrencyCode = currency,
                        Value = amount.ToString("F2")
                    }
                });

                var response = await _client.Execute(request);
                var result = response.Result<PayPalCheckoutSdk.Payments.Refund>();

                _logger.LogInformation(
                    "PayPal refund created: {RefundId} for capture {CaptureId}",
                    result.Id,
                    captureId);

                return (true, result.Id, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding PayPal capture {CaptureId}", captureId);
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Verifies PayPal webhook signature authenticity
        /// </summary>
        public async Task<bool> VerifyWebhookSignatureAsync(
            string transmissionId,
            string transmissionTime,
            string certUrl,
            string authAlgo,
            string transmissionSig,
            string webhookId,
            string webhookEvent)
        {
            try
            {
                // Download certificate from PayPal
                var certificateData = await _httpClient.GetByteArrayAsync(certUrl);
                var certificate = new X509Certificate2(certificateData);

                // Build the expected signature string
                var expectedSignature = $"{transmissionId}|{transmissionTime}|{webhookId}|{webhookEvent}";

                // Decode the signature from base64
                var signatureBytes = Convert.FromBase64String(transmissionSig);

                // Get the public key from certificate
                var publicKey = certificate.GetRSAPublicKey();
                if (publicKey == null)
                {
                    _logger.LogWarning("Failed to extract RSA public key from PayPal certificate");
                    return false;
                }

                // Verify the signature
                var isValid = publicKey.VerifyData(
                    Encoding.UTF8.GetBytes(expectedSignature),
                    signatureBytes,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                if (isValid)
                {
                    _logger.LogInformation("PayPal webhook signature verified successfully");
                }
                else
                {
                    _logger.LogWarning("PayPal webhook signature verification failed");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying PayPal webhook signature");
                return false;
            }
        }
    }
}