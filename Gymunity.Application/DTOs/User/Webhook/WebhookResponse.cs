namespace Gymunity.Application.DTOs.User.Webhook
{
    public class WebhookResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int? PaymentId { get; set; }
    }
}