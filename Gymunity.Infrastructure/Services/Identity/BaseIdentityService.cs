using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.DTOs.Account;
using Gymunity.Application.DTOs.Email;
using Gymunity.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Gymunity.Application.Mapping;

namespace Gymunity.Infrastructure.Services.Identity
{
    public abstract class BaseIdentityService(
        IEmailService emailService,
        IImageUrlResolver imageUrlResolver,
        UserManager<AppUser> userManager)
    {
        protected async Task SendStatusEmailAsync(AppUser user, string subject, string message)
        {
            await emailService.SendEmailAsync(new EmailRequest
            {
                ToEmail = user.Email!,
                ToName = user.UserName ?? "",
                Subject = subject,
                Body = message
            });
        }
        protected AuthResponse PrepareAuthResponseAsync(AppUser user, string token)
        {
            var photoUrl = imageUrlResolver.ResolveImageUrl(user.ProfilePhotoUrl ?? "");
            return user.ToUserResponse(token, photoUrl);
        }
        protected async Task<bool> IsEmailUniqueAsync(string email) =>
            await userManager.FindByEmailAsync(email) is null;
        protected async Task<bool> IsUserNameUniqueAsync(string username) =>
            await userManager.FindByNameAsync(username) is null;
    }
}