using Gymunity.Application.DTOs.Account;

namespace Gymunity.Application.Contracts.Services.Identity
{
    public interface IAccountService
    {
        Task<AuthResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request);
        Task<AuthResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    }
}