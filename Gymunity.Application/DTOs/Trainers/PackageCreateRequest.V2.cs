using Gymunity.Application.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gymunity.Application.DTOs.Trainers
{
    public class PackageCreateRequestV2
    {
        [Required(ErrorMessage = "Name is required and must be between3 and100 characters.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between3 and100 characters.")]
        [UniquePackageName]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description must be at most500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Monthly price is required and must be between0.01 and100000.")]
        [Range(0.01, 100000, ErrorMessage = "Monthly price must be between0.01 and100000.")]
        public decimal PriceMonthly { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Yearly price must be between0.01 and100000.")]
        public decimal? PriceYearly { get; set; }

        public bool IsActive { get; set; } = true;
        public string? ThumbnailUrl { get; set; }
        public int[] ProgramIds { get; set; } = new int[0];

        // New: accept program titles instead of ids
        public string[] ProgramNames { get; set; } = new string[0];

        // New
        public bool IsAnnual { get; set; }
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Promo code must be between3 and20 characters.")]
        public string? PromoCode { get; set; }

        // TrainerProfileId is the TrainerProfile.Id (int)
        [JsonPropertyName("trainerProfileId")]
        public int TrainerProfileId { get; set; }

        // Accept legacy JSON field name 'trainerId' (int) for backward compatibility
        [JsonPropertyName("trainerId")]
        public int TrainerIdLegacy
        {
            set => TrainerProfileId = value;
        }
    }
}