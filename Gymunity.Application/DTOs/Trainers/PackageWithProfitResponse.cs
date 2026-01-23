namespace Gymunity.Application.DTOs.Trainers
{
    public class PackageWithProfitResponse
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal PriceMonthly { get; set; }
        public decimal? PriceYearly { get; set; }
        public string Currency { get; set; } = "USD";
        public int? TrainerProfileId { get; set; }
        public string? TrainerUserId { get; set; }
        public string? TrainerName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // New metrics
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PlatformFee { get; set; }

        // Existing profit (numeric)
        public decimal Profit { get; set; }

        // Formatted strings for UI convenience
        public string TotalRevenueFormatted => FormatCurrency(TotalRevenue, Currency);
        public string PlatformFeeFormatted => FormatCurrency(PlatformFee, Currency);
        public string ProfitFormatted => FormatCurrency(Profit, Currency);

        private static string FormatCurrency(decimal amount, string currency)
        {
            // Use invariant culture for consistent decimal point, format with2 decimals
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} {1}", amount, currency);
        }
    }
}