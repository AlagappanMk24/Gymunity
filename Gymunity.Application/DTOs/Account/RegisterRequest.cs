using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Account
{
    public record RegisterRequest
    {
        [Description("Required, Minimum Length is 3 characters!")]
        [Required]
        [MinLength(3)]
        public string UserName { get; set; } = null!;
        [Description("Required, Must be a valid email format!")]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Description("Required, Minimum Length is 8 characters, Must contain at least one uppercase letter, one lowercase letter, one digit, and one special character!")]
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()\-_=+{}[\]|;:'"",.<>\/\\]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; } = null!;
        [Description("Required, Must match the Password field!")]
        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
        [Description("Required, Minimum Length is 3 characters!")]
        [Required]
        [MinLength(3)]
        public string FullName { get; set; } = null!;
        [Description("Required, Must be a valid Image file!")]
        [Required]
        public IFormFile ProfilePhoto { get; set; } = null!;
        [Description("Required, 1 for Client User, 2 for Trainer!")]
        [Required]
        [Range(1,2)]
        public byte Role { get; set; }
    }
}