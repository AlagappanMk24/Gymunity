using Gymunity.Application.DTOs.Account.OTP;
using Gymunity.Application.DTOs.Auth;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Enums;

namespace Gymunity.Application.Contracts.Services.Identity
{
    public interface IIdentityService
    {
        // Registration Flow 
        Task<InitiateRegistrationResponse> InitiateRegistrationAsync(RegisterRequest request);
        Task<AuthResponse> CompleteRegistrationAsync(string email, string otpCode);
        Task<OtpResponse> ResendRegistrationOtpAsync(string email);

        // Login Flow
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<OtpResponse> SendLoginOtpAsync(string email);

        // OTP Verification
        OtpResponse VerifyRegistrationOtp(string email, string otpCode);

        // Token Generation
        Task<string> CreateTokenAsync(AppUser user);

        event Func<string, string, string, UserRole, Task>? NewUserRegisteredAsync;
    }
}