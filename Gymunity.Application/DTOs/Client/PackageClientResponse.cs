namespace Gymunity.Application.DTOs.Client
{
    public class PackageClientResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public decimal PriceMonthly { get; set; }
        public decimal? PriceYearly { get; set; }
        public bool IsActive { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string TrainerId { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsAnnual { get; set; }
        public string? PromoCode { get; set; }

        // New: brief program info for guest (no ids)
        public ProgramBriefResponse[]? Programs { get; set; }
    }
}
