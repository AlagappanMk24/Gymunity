namespace Gymunity.Application.DTOs.Trainers
{
    public class TrainerSubscriptionItem
    {
        public int Id { get; set; }
        public int PackageId { get; set; }
        public string? PackageName { get; set; }

        // Client info
        public string ClientId { get; set; } = null!;
        public string? ClientName { get; set; }
        public string? ClientEmail { get; set; }

        // Subscription details
        public string Status { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; } = "USD";
        public string SubscriptionType { get; set; } = "Monthly"; // Monthly or Annual
    }
}