using Gymunity.Application.DTOs.Account;

namespace Gymunity.Application.Contracts.Services.Identity
{
    public interface IPasswordService
    {
        Task<bool> SendResetPasswordLinkAsync(ForgetPasswordRequest request);
        Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
    }
}