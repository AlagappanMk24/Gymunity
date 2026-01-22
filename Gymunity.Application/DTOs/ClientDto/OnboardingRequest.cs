using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.ClientDto
{
    public class OnboardingRequest
    {
        [Range(50, 300, ErrorMessage = "Height must be between 50 and 300 cm")]
        public int? HeightCm { get; set; }

        [Range(20, 500, ErrorMessage = "Starting weight must be between 20 and 500 kg")]
        public decimal? StartingWeightKg { get; set; }

        [Required(ErrorMessage = "Goal is required")]
        [RegularExpression("^(Fat Loss|Muscle Gain|Maintenance)$",
            ErrorMessage = "Goal must be Fat Loss, Muscle Gain, or Maintenance")]
        public string? Goal { get; set; }

        [Required(ErrorMessage = "Experience level is required")]
        [RegularExpression("^(Beginner|Intermediate|Advanced)$",
            ErrorMessage = "Experience level must be Beginner, Intermediate, or Advanced")]
        public string? ExperienceLevel { get; set; }
    }
}