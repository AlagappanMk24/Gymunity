using Gymunity.Application.DTOs.Account;

namespace Gymunity.Application.Contracts.ExternalServices.Auth
{
    public interface IGoogleAuthService
    {
        Task<AuthResponse> GoogleAuthAsync(GoogleAuthRequest request);
        Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken);
    }

    public class GoogleUserInfo
    {
        public string GoogleId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Picture { get; set; }
    }
}
