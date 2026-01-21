namespace Gymunity.Application.DTOs.Account
{
    public class AuthResponse
    {
        // expose Id so clients (Postman) can receive user identifier after login/register
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = null!;
        public string? ProfilePhotoUrl { get; set; }
        public string Token { get; set; } = null!;

    }
}