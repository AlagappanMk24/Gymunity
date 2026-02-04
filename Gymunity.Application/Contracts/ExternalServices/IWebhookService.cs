using Gymunity.Application.DTOs.User.Webhook;

namespace Gymunity.Application.Contracts.ExternalServices
{
    public interface IWebhookService
    {
        Task<WebhookResponse> ProcessPaymobWebhookAsync(
            PaymobWebhookPayload payload,
            string receivedHmac);
        Task<WebhookResponse> ProcessPayPalWebhookAsync(
            PayPalWebhookPayload payload,
            string transmissionId,
            string transmissionTime,
            string certUrl,
            string authAlgo,
            string transmissionSig);
        Task<WebhookResponse> ProcessStripeWebhookAsync(
           string jsonPayload,
           string signatureHeader);
    }
}
