using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Auth
{
    public record CompleteRegistrationRequest
    {
        [Description("Required, Must be the same email used in registration initiation!")]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Description("Required, 6-digit OTP code!")]
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = null!;
    }
}