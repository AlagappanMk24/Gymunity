namespace Gymunity.Application.DTOs.Email
{
    public class SubscriptionConfirmationEmail
    {
        public string ClientName { get; set; } = null!;
        public string ClientEmail { get; set; } = null!;
        public string PackageName { get; set; } = null!;
        public string TrainerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime SubscriptionEndDate { get; set; }
    }
}