using Gymunity.Domain.Enums;

namespace Gymunity.Application.DTOs.Client
{
    public class ClientSubscriptionGetResponse
    {
        public int SubscriptionId { get; set; }
        public int PackageId { get; set; }
        public SubscriptionStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
    }
}
