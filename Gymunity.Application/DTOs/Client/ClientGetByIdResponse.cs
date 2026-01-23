namespace Gymunity.Application.DTOs.Client
{
    public class ClientGetByIdResponse
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? ProfilePhotoUrl { get; set; }
        public string? Role { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
        public string? StripeCustomerId { get; set; }
    }
}
