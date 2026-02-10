using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Account.OTP
{
    public record SendOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Description("Purpose: 'login', 'register', 'reset-password', 'change-email'")]
        [Required]
        public string Purpose { get; set; } = null!;
    }
}