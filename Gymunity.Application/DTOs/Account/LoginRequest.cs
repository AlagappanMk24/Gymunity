using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Account
{
    public class LoginRequest
    {
        [Required]
        public string EmailOrUserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}