using Gymunity.Application.DTOs.Account;
using Gymunity.Application.DTOs.Account.OTP;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Gymunity.Application.Contracts.Services.Identity
{
    public interface IIdentityService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<string> CreateTokenAsync(AppUser user);
        Task<OtpResponse> SendRegistrationOtpAsync(string email);
        OtpResponse VerifyRegistrationOtp(string email, string otpCode);
        Task<OtpResponse> SendLoginOtpAsync(string email);
        Task<AuthResponse> CompleteRegistrationWithOtpAsync(CompleteRegistrationRequest request);

        event Func<string, string, string, UserRole, Task>? NewUserRegisteredAsync;
    }
}