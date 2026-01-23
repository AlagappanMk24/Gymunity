using Gymunity.Application.DTOs.Packages;

namespace Gymunity.Application.DTOs.Trainers
{
    public class PackageWithSubscriptionsResponse
    {
        public PackageResponse Package { get; set; } = null!;

        // public List<SubscriptionResponse> Subscriptions { get; set; } = new List<SubscriptionResponse>();

        public List<TrainerSubscriptionItem> Subscriptions { get; set; } = new List<TrainerSubscriptionItem>();
    }
}