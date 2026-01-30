namespace Gymunity.Application.Configuration
{
    public class StripeSettings
    {
        public string SecretKey { get; set; } = null!;
        public string PublishableKey { get; set; } = null!;
        public string WebhookSecret { get; set; } = null!;
    }
}