using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Auth
{
    public record VerifyOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = null!;

        [Required]
        public string Purpose { get; set; } = null!;
    }
}