using Gymunity.Application.DTOs.Account;
using Gymunity.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Gymunity.Application.Contracts.Services.Identity
{
    public interface IIdentityService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<string> CreateTokenAsync(AppUser user, UserManager<AppUser> userManager);
    }
}