using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.Services;
using Gymunity.Application.DTOs.Email;
using Gymunity.Application.DTOs.User.Webhook;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Controllers
{
    /// <summary>
    /// Controller for handling third-party payment gateways webhooks (Paymob, PayPal, Stripe).
    /// </summary>
    /// <param name="webhookService">Service for processing webhook payloads.</param>
    /// <param name="emailService">Service for sending automated emails after payment events.</param>
    /// <param name="logger">Logging instance.</param>
    [ApiController]
    [Route("api/webhooks")]
    [Produces("application/json")]
    public class WebhooksController(
        IWebhookService webhookService,
        IEmailTemplateService emailService, 
        ILogger<WebhooksController> logger) : ControllerBase
    {
        private readonly IWebhookService _webhookService = webhookService;
        private readonly IEmailTemplateService _emailService = emailService; 
        private readonly ILogger<WebhooksController> _logger = logger;

        /// Paymob webhook endpoint
        /// Called by Paymob after payment completion/failure
        [HttpPost("paymob")]
        [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PaymobWebhook([FromBody] PaymobWebhookPayload payload)
        {
            // Get HMAC from query string (Paymob convention)
            var hmac = Request.Query["hmac"].ToString();

            _logger.LogInformation(
                "📥 Paymob webhook received | Transaction: {TransactionId} | Order: {OrderId} | Success: {Success}",
                payload.Obj?.Id,
                payload.Obj?.Order?.Merchant_Order_Id,
                payload.Obj?.Success);

            // Log request details for debugging
            _logger.LogDebug(
                "Paymob webhook details: Amount={Amount}, Currency={Currency}, HMAC={HmacPresent}",
                payload.Obj?.Amount_Cents,
                payload.Obj?.Currency,
                !string.IsNullOrEmpty(hmac));

            //  Process webhook (this already sends emails internally)
            var result = await _webhookService.ProcessPaymobWebhookAsync(payload, hmac);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "❌ Paymob webhook failed | Order: {OrderId} | Reason: {Reason}",
                    payload.Obj?.Order?.Merchant_Order_Id,
                    result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation(
                "✅ Paymob webhook processed | Payment: {PaymentId} | Message: {Message}",
                result.PaymentId,
                result.Message);

            //  Note: Emails are already sent in WebhookService.CompletePaymentAsync()
            // No need to send here unless you want additional notifications

            return Ok(result);
        }

        /// PayPal webhook endpoint
        /// Called by PayPal for various payment events
        [HttpPost("paypal")]
        [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PayPalWebhook([FromBody] PayPalWebhookPayload payload)
        {
            _logger.LogInformation(
                "📥 PayPal webhook received | Event: {EventType} | ID: {EventId}",
                payload.Event_Type,
                payload.Id);

            // Log additional details
            _logger.LogDebug(
                "PayPal webhook details: Resource={ResourceId}, Status={Status}",
                payload.Resource?.Id,
                payload.Resource?.Status);

            // Extract signature headers for verification
            var transmissionId = Request.Headers["Paypal-Transmission-Id"].ToString();
            var transmissionTime = Request.Headers["Paypal-Transmission-Time"].ToString();
            var certUrl = Request.Headers["Paypal-Cert-Url"].ToString();
            var authAlgo = Request.Headers["Paypal-Auth-Algo"].ToString();
            var transmissionSig = Request.Headers["Paypal-Transmission-Sig"].ToString();

            var result = await _webhookService.ProcessPayPalWebhookAsync(
                payload,
                transmissionId,
                transmissionTime,
                certUrl,
                authAlgo,
                transmissionSig);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "❌ PayPal webhook failed | Event: {EventType} | Reason: {Reason}",
                    payload.Event_Type,
                    result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation(
                "✅ PayPal webhook processed | Payment: {PaymentId} | Message: {Message}",
                result.PaymentId,
                result.Message);

            return Ok(result);
        }

        /// <summary>
        /// Stripe webhook endpoint
        /// Called by Stripe for payment_intent events
        /// </summary>
        [HttpPost("stripe")]
        [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StripeWebhook()
        {
            try
            {
                // Read raw request body
                var json = await new StreamReader(Request.Body).ReadToEndAsync();
                var signature = Request.Headers["Stripe-Signature"].ToString();

                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Missing Stripe-Signature header in webhook request");
                    return BadRequest(new WebhookResponse
                    {
                        Success = false,
                        Message = "Missing signature header"
                    });
                }

                _logger.LogInformation("📥 Stripe webhook received | Signature present");

                var result = await _webhookService.ProcessStripeWebhookAsync(json, signature);

                if (!result.Success)
                {
                    _logger.LogWarning("❌ Stripe webhook failed | Reason: {Reason}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation(
                    "✅ Stripe webhook processed | Payment: {PaymentId} | Message: {Message}",
                    result.PaymentId,
                    result.Message);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return StatusCode(500, new WebhookResponse
                {
                    Success = false,
                    Message = "Internal error processing webhook"
                });
            }
        }

        /// Test endpoint for webhook validation (Development only!)
        [HttpGet("test")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult TestWebhook()
        {
            return Ok(new
            {
                status = "Webhook endpoints are active",
                endpoints = new
                {
                    paymob = $"{Request.Scheme}://{Request.Host}/api/webhooks/paymob",
                    paypal = $"{Request.Scheme}://{Request.Host}/api/webhooks/paypal",
                    stripe = $"{Request.Scheme}://{Request.Host}/api/webhooks/stripe"
                },
                timestamp = DateTime.UtcNow,
                features = new
                {
                    emailNotifications = "Enabled",
                    hmacVerification = "Enabled",
                    stripeSignatureVerification = "Enabled",
                    rateLimiting = "Enabled"
                }
            });
        }

        ///  NEW: Manual email test endpoint (Development only!)
        [HttpPost("test/send-confirmation-email")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> TestSendConfirmationEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email parameter is required" });

            try
            {
                var testData = new SubscriptionConfirmationEmail
                {
                    ClientName = "Test Client",
                    ClientEmail = email,
                    PackageName = "Pro Training Package",
                    TrainerName = "Coach Ahmed",
                    Amount = 499.00m,
                    Currency = "EGP",
                    SubscriptionStartDate = DateTime.UtcNow,
                    SubscriptionEndDate = DateTime.UtcNow.AddMonths(1)
                };

                var success = await _emailService.SendSubscriptionConfirmationAsync(testData);

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Test confirmation email sent to {email}"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"Failed to send email to {email}. Check logs for details."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing email send to {Email}", email);
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        ///  NEW: Manual trainer notification test (Development only!)
        [HttpPost("test/send-trainer-notification")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> TestSendTrainerNotification([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email parameter is required" });

            try
            {
                var testData = new TrainerNotificationEmail
                {
                    TrainerName = "Coach Ahmed",
                    TrainerEmail = email,
                    ClientName = "John Doe",
                    PackageName = "Pro Training Package",
                    Amount = 499.00m,
                    TrainerPayout = 424.15m,
                    Currency = "EGP"
                };

                var success = await _emailService.SendTrainerNotificationAsync(testData);

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Test trainer notification sent to {email}"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"Failed to send email to {email}. Check logs for details."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing trainer email send to {Email}", email);
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        ///  NEW: Test payment failure email (Development only!)
        [HttpPost("test/send-failure-email")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> TestSendFailureEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email parameter is required" });

            try
            {
                var success = await _emailService.SendPaymentFailureEmailAsync(
                    email,
                    "Test Client",
                    "Pro Training Package",
                    "Insufficient funds - This is a test");

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Test failure email sent to {email}"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"Failed to send email to {email}. Check logs for details."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing failure email send to {Email}", email);
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
    }
}