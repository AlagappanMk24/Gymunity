using Gymunity.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.ClientDto
{
    public class ClientProfileRequest
    {
        //public int Id { get; set; }
        [Required(ErrorMessage = "User name is required")]
        [StringLength(50, MinimumLength = 3,
        ErrorMessage = "User name must be between 3 and 50 characters")]
        public string UserName { get; set; } = null!;

        [Range(50, 300, ErrorMessage = "Height must be between 50 and 300 cm")]
        public int? HeightCm { get; set; }

        [Range(20, 500, ErrorMessage = "Weight must be between 20 and 500 kg")]
        public decimal? StartingWeightKg { get; set; }

        [Description("1 for Male, 2 for Female")]
        public Gender? Gender { get; set; }
        public ClientGoal? Goal { get; set; }  // "Fat Loss", "Muscle Gain", etc.
        public ExperienceLevel? ExperienceLevel { get; set; }  // Beginner, Intermediate, Advanced
    }
}