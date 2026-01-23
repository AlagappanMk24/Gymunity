using Gymunity.Domain.Enums;

namespace Gymunity.Application.DTOs.Trainers
{
    public class SubscriberResponse
    {
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime SubscriptionEndDate { get; set; }
        public SubscriptionStatus Status { get; set; }
    }
}