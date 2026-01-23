using Gymunity.Application.DTOs.Program;

namespace Gymunity.Application.DTOs.Packages
{
    public class PackageResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public decimal PriceMonthly { get; set; }
        public decimal? PriceYearly { get; set; }
        public bool IsActive { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string TrainerId { get; set; } = null!;
        public bool IsAnnual { get; set; }
        public string? PromoCode { get; set; }
        public int[] ProgramIds { get; set; } = new int[0];

        // Include full program details
        public ProgramGetAllResponse[]? Programs { get; set; } = null;
    }
}