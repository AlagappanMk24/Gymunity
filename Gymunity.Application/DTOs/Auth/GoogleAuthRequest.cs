using Gymunity.Domain.Enums;

namespace Gymunity.Application.DTOs.Auth
{
    public record GoogleAuthRequest
    {
        public string IdToken { get; set; } // Google ID token from frontend
        public UserRole? Role { get; set; }
    }
}