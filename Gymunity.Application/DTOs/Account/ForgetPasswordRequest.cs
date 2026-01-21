using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Account
{
    public class ForgetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}