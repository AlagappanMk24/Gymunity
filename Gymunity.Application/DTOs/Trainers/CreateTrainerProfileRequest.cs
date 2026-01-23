using Gymunity.Application.Validation;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Trainers
{
    public class CreateTrainerProfileRequest
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        [StringLength(50)]
        [UniqueTrainerHandle]
        public string Handle { get; set; } = null!;

        [StringLength(500)]
        public string Bio { get; set; } = string.Empty;

        public IFormFile? CoverImage { get; set; }

        [Url]
        public string? VideoIntroUrl { get; set; }

        [StringLength(100)]
        public string? BrandingColors { get; set; }

        [Range(0, 50)]
        public int YearsExperience { get; set; }
    }
}