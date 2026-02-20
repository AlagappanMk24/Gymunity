using Gymunity.Domain.Enums;

namespace Gymunity.Application.DTOs.Auth
{
    public record AuthResponse
    {
        // expose Id so clients (Postman) can receive user identifier after login/register
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string Token { get; set; } = null!;

        // OTP specific properties
        public bool RequiresOtp { get; set; }
        public string? Message { get; set; }
        public DateTime? OtpExpiresAt { get; set; }
        public bool IsAccountActive { get; set; }

    }
}