namespace Gymunity.Application.Configuration
{
    public class PayPalSettings
    {
        public string Mode { get; set; } = "Sandbox"; // "Sandbox" or "Live"
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string ReturnUrl { get; set; } = null!;
        public string CancelUrl { get; set; } = null!;
        public string WebhookId { get; set; } = null!;  // For webhook signature verification
    }
}