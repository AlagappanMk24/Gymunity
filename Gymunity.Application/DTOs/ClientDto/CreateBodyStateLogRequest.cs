using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.ClientDto
{
    public class CreateBodyStateLogRequest
    {
        [Range(20, 500, ErrorMessage = "Weight must be between 20 and 500 kg")]
        public decimal? WeightKg { get; set; }

        [Range(1, 80, ErrorMessage = "Body fat percentage must be between 1 and 80")]
        public decimal? BodyFatPercent { get; set; }

        [StringLength(2000, ErrorMessage = "Measurements data is too long")]
        public string? MeasurementsJson { get; set; } // Example: { "neck": 40, "waist": 80 }

        [Url(ErrorMessage = "Front photo must be a valid URL")]
        [StringLength(500)]
        public string? PhotoFrontUrl { get; set; }

        [Url(ErrorMessage = "Side photo must be a valid URL")]
        [StringLength(500)]
        public string? PhotoSideUrl { get; set; }

        [Url(ErrorMessage = "Back photo must be a valid URL")]
        [StringLength(500)]
        public string? PhotoBackUrl { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

    }
}