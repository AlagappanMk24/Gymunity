using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DTOs.Account;
using Gymunity.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gymunity.Infrastructure.Services.Identity
{
    public class PasswordService(UserManager<AppUser> userManager, IEmailService emailService, IIdentityService identityService, IImageUrlResolver imageUrlResolver,
        IConfiguration configuration, ILogger<PasswordService> logger) : BaseIdentityService(emailService, imageUrlResolver, userManager), IPasswordService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IIdentityService _identityService = identityService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<PasswordService> _logger = logger;
        public async Task<bool> SendResetPasswordLinkAsync(ForgetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                _logger.LogWarning("ForgetPassword requested for non-existing email: {Email}", request.Email);
                return false;
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var baseUrl = _configuration["FrontendOrigins:Local"] ?? _configuration["FrontendOrigins:Hosted"];
            var resetLink = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

            await SendStatusEmailAsync(user, "Password Reset Request", $"Click the link to reset your password: {resetLink}");
            return true;
        }
        public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
                ?? throw new Exception("Invalid email address.");
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Password reset failed: {errors}");
            }
            var token = await _identityService.CreateTokenAsync(user, userManager);
            await SendStatusEmailAsync(user, "Password Reset Success", "You succesfully Reset your password in Gymunity!.");
            return PrepareAuthResponseAsync(user, token);
        }
    }
}