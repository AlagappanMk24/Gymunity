using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Client;
using Gymunity.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Services.Trainer
{
    public class ClientService(UserManager<AppUser> userManager) : IClientService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        public async Task<IReadOnlyList<ClientGetAllResponse>> GetAllByTrainerIdAsync(string trainerId)
        {
            // Replace with real logic linking clients to trainer (subscriptions, packages, etc.)
            var users = await _userManager.Users.ToListAsync();
            return users.Select(u => new ClientGetAllResponse { UserId = u.Id, UserName = u.UserName ?? string.Empty, ProfilePhotoUrl = u.ProfilePhotoUrl, Role = u.Role.ToString() }).ToList();
        }

        public async Task<ClientGetByIdResponse?> GetByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            return new ClientGetByIdResponse { UserId = user.Id, UserName = user.UserName ?? string.Empty, ProfilePhotoUrl = user.ProfilePhotoUrl, Role = user.Role.ToString(), LastLoginAt = user.LastLoginAt, StripeCustomerId = user.StripeCustomerId };
        }
    }
}