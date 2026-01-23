using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Trainers
{
    public class UpdateTrainerProfileRequest
    {
        [StringLength(50)]
        public string? Handle { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public IFormFile? CoverImage { get; set; }

        [Url]
        public string? VideoIntroUrl { get; set; }

        [StringLength(100)]
        public string? BrandingColors { get; set; }

        [Range(0, 50)]
        public int? YearsExperience { get; set; }
    }
}