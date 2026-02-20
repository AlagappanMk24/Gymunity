using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Auth
{
    public class ForgetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}