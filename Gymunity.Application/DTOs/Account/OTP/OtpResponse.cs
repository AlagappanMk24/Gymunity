namespace Gymunity.Application.DTOs.Account.OTP
{
    public record OtpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
        public int AttemptsRemaining { get; set; } = 3;
    }
}