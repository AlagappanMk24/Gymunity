namespace Gymunity.Application.DTOs.User.Subscriptions
{
    public class SubscriptionListResponse
    {
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public List<SubscriptionResponse> Subscriptions { get; set; } = new();
    }
}