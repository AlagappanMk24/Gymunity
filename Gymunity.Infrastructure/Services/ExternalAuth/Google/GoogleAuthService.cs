using Google.Apis.Auth;
using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.ExternalServices.Auth;
using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DTOs.Account;
using Gymunity.Application.DTOs.Email;
using Gymunity.Application.Mapping;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gymunity.Infrastructure.Services.ExternalAuth.Google
{
    public class GoogleAuthService(UserManager<AppUser> userManager , IIdentityService identityService, IEmailService emailService,IConfiguration configuration, ILogger<GoogleAuthService> logger, IImageUrlResolver imageUrlResolver) : IGoogleAuthService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IIdentityService _identityService = identityService;
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<GoogleAuthService> _logger = logger;
        private readonly IImageUrlResolver _imageUrlResolver = imageUrlResolver;

        public event Func<string, string, string, UserRole, Task>? NewGoogleUserRegisteredAsync;
        public async Task<AuthResponse> GoogleAuthAsync(GoogleAuthRequest request)
        {
            // Validate Google token
            var googleUser = await ValidateGoogleTokenAsync(request.IdToken)
                ?? throw new Exception("Invalid Google token");

            if (!googleUser.EmailVerified)
                throw new Exception("Google email is not verified");


            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(googleUser.Email);

            AppUser user;

            if (existingUser != null)
            {
                // User exists - Login
                user = existingUser;

                // Check if Google login is already linked
                var logins = await _userManager.GetLoginsAsync(user);
                var googleLogin = logins.FirstOrDefault(l => l.LoginProvider == "Google");

                if (googleLogin == null)
                {
                    // Link Google account to existing user
                    var addLoginResult = await _userManager.AddLoginAsync(user,
                        new UserLoginInfo("Google", googleUser.GoogleId, "Google"));

                    if (!addLoginResult.Succeeded)
                    {
                        _logger.LogError("Failed to link Google account for user {Email}", googleUser.Email);
                    }
                }
            }
            else
            {
                // User doesn't exist - Register
                user = new AppUser
                {
                    FullName = $"{googleUser.FirstName} {googleUser.LastName}",
                    ProfilePhotoUrl = googleUser.Picture,
                    Email = googleUser.Email,
                    UserName = googleUser.Email,
                    Role = request.Role ?? UserRole.Client,
                    EmailConfirmed = true // Google email is already verified
                };

                // Create user without password (external login)
                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create user: {errors}");
                }


                // Add external login info
                var addLoginResult = await _userManager.AddLoginAsync(user,
                    new UserLoginInfo("Google", googleUser.GoogleId, "Google"));

                if (!addLoginResult.Succeeded)
                {
                    _logger.LogError("Failed to add Google login for new user {Email}", googleUser.Email);
                }

                var roleResult = await _userManager.AddToRoleAsync(user, request.Role.ToString() ?? "Client");
                // Assign default role
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to assign role to user {Email}", googleUser.Email);
                }

                // ✅ Raise event for notification handlers
                if (NewGoogleUserRegisteredAsync != null)
                {
                    try
                    {
                        await NewGoogleUserRegisteredAsync(user.Id, user.FullName, user.Email, user.Role);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Event notification failed for new Google user registration {UserId}", user.Id);
                        // Don't rethrow - registration already succeeded
                    }
                }
            }

            // Generate JWT token
            var token = await _identityService.CreateTokenAsync(user, _userManager);

            var emailRequset = new EmailRequest()
            {
                ToEmail = user.Email,
                ToName = user.UserName ?? "",
                Subject = "Login Success",
                Body = "You succesfully Signed in to Gymunity!"
            };

            await _emailService.SendEmailAsync(emailRequset);

            return user.ToUserResponse(token, _imageUrlResolver.ResolveImageUrl(user.ProfilePhotoUrl ?? ""));
        }
        public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                var clientId = _configuration["Authentication:Google:ClientId"];

                if (string.IsNullOrEmpty(clientId))
                {
                    _logger.LogError("Google ClientId is not configured");
                    return null;
                }

                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

                _logger.LogInformation("Successfully validated Google token for email: {Email}", payload.Email);

                return new GoogleUserInfo
                {
                    GoogleId = payload.Subject,
                    Email = payload.Email,
                    EmailVerified = payload.EmailVerified,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    Picture = payload.Picture
                };
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogError(ex, "Invalid JWT token");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate Google token");
                return null;
            }
        }
    }
}