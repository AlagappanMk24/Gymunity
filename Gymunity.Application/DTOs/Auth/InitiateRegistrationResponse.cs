namespace Gymunity.Application.DTOs.Auth
{
    public record InitiateRegistrationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? OtpExpiresAt { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}