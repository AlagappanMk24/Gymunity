using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DTOs.Account;
using Gymunity.Application.DTOs.Email;
using Gymunity.Application.Mapping;
using Gymunity.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Gymunity.Infrastructure.Services.Identity
{
    public class AccountService(UserManager<AppUser> userManager, IIdentityService identityService, IFileUploadService fileUploadService, IEmailService emailService,
        IImageUrlResolver imageUrlResolver) : BaseIdentityService(emailService, imageUrlResolver,userManager), IAccountService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IIdentityService _identityService = identityService;
        private readonly IFileUploadService _fileUploadService = fileUploadService;
        public async Task<AuthResponse> UpdateProfileAsync(string? userId, UpdateProfileRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId ?? "") ?? throw new Exception("User not found.");

            if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
                user.Email = await IsEmailUniqueAsync(request.Email) ? request.Email : throw new Exception("Email is already registered.");
           
            if (!string.Equals(user.UserName, request.UserName, StringComparison.OrdinalIgnoreCase))
            {
                if (!await IsUserNameUniqueAsync(request.UserName))
                {
                    throw new Exception("Username is already taken.");
                }
                user.UserName = request.UserName;
            }

            user.FullName = request.FullName;

            if (request.ProfilePhoto is not null)
            {
                if (!_fileUploadService.IsValidImageFile(request.ProfilePhoto))
                {
                    throw new Exception("Invalid profile photo format.");
                }
                // Delete old photo if exists
                var photoPath = await _fileUploadService.UploadImageAsync(request.ProfilePhoto, IFileUploadService.UserProfilePhotosFolder);
                if (!string.IsNullOrEmpty(photoPath))
                {
                    var isDeleted = user.ProfilePhotoUrl is not null && _fileUploadService.DeleteImage(user.ProfilePhotoUrl);
                }
                user.ProfilePhotoUrl = photoPath;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Profile update failed: {errors}");
            }

            var token = await _identityService.CreateTokenAsync(user, userManager);
            await SendStatusEmailAsync(user, "Profile Update Success", "You succesfully Updated your profile in Gymunity!.");
            return  PrepareAuthResponseAsync(user, token);
        }
        public async Task<AuthResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("Invalid Operation.");

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Password update failed: {errors}");
            }
            var token = await _identityService.CreateTokenAsync(user, userManager);
            await SendStatusEmailAsync(user, "Password Change Success", "You succesfully Changed your password in Gymunity!.");
            return PrepareAuthResponseAsync(user, token);
        }
    }
}