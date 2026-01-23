using Gymunity.Application.Validation;
using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Packages
{
    public class PackageCreateRequest
    {
        [Required(ErrorMessage = "Name is required and must be between 3 and 100 characters.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
        [UniquePackageName]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description must be at most 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Monthly price is required and must be between 0.01 and 100000.")]
        [Range(0.01, 100000, ErrorMessage = "Monthly price must be between 0.01 and 100000.")]
        public decimal PriceMonthly { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Yearly price must be between 0.01 and 100000.")]
        public decimal? PriceYearly { get; set; }

        public bool IsActive { get; set; } = true;
        public string? ThumbnailUrl { get; set; }
        public int[] ProgramIds { get; set; } = new int[0];

        // New
        public bool IsAnnual { get; set; }
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Promo code must be between 3 and 20 characters.")]
        public string? PromoCode { get; set; }

        // TrainerId added so controller can accept trainer id in request for dev/testing
        [Required(ErrorMessage = "TrainerId is required.")]
        public int TrainerId { get; set; }
    }
}