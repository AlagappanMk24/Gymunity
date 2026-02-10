using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Account
{
    public record CompleteRegistrationRequest
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = null!;

        [Required]
        public RegisterRequest RegistrationData { get; set; } = null!;
    }
}