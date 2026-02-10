namespace Gymunity.Application.DTOs.Account.OTP
{
    public class OtpRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
        public string Purpose { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public int Attempts { get; set; }
    }
}