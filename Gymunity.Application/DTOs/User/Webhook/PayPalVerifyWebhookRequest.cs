namespace Gymunity.Application.DTOs.User.Webhook
{
    public class PayPalWebhookVerifyRequest
    {
        public string TransmissionId { get; set; } = null!;
        public string TransmissionTime { get; set; } = null!;
        public string CertUrl { get; set; } = null!;
        public string AuthAlgo { get; set; } = null!;
        public string TransmissionSig { get; set; } = null!;
        public string WebhookId { get; set; } = null!;
        public string WebhookEvent { get; set; } = null!;
    }
}