using Gymunity.Application.DTOs.Account;
using Gymunity.Application.DTOs.Auth;

namespace Gymunity.Application.Contracts.Services.Identity
{
    public interface IAccountService
    {
        Task<AuthResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request);
        Task<AuthResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    }
}