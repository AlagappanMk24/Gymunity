using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DTOs.Account;
using Gymunity.Application.DTOs.Email;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gymunity.Application.Mapping;
using Gymunity.Application.Contracts.ExternalServices.Auth;

namespace Gymunity.Infrastructure.Services.Identity
{
    public class IdentityService(UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager, IFileUploadService fileUploadService,
        IEmailService emailService,
        IConfiguration configuration,
        IImageUrlResolver imageUrlResolver) : BaseIdentityService(emailService, imageUrlResolver, userManager), IIdentityService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly IFileUploadService _fileUploadService = fileUploadService;
        private readonly IConfiguration _configuration = configuration;

        // ✅ Observable events for notification handlers
        public event Func<string, string, string, UserRole, Task>? NewUserRegisteredAsync;
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.EmailOrUserName)
                       ?? await _userManager.FindByNameAsync(request.EmailOrUserName)
                       ?? throw new Exception("Invalid credentials.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
                throw new Exception("Invalid email/username or password.");
            var token = await CreateTokenAsync(user, userManager);
            await SendStatusEmailAsync(user, "Login Success", "You succesfully Signed in to Gymunity!");
            return PrepareAuthResponseAsync(user, token);
        }
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (!await IsEmailUniqueAsync(request.Email)) throw new Exception("Email is already registered.");
            if (!await IsUserNameUniqueAsync(request.UserName)) throw new Exception("Handle is already taken.");
            var user = new AppUser
            {
                UserName = request.UserName.ToLower(),
                Email = request.Email,
                FullName = request.FullName,
                Role = (UserRole)request.Role,
            };
            if (request.ProfilePhoto != null)
            {
                if (!_fileUploadService.IsValidImageFile(request.ProfilePhoto)) throw new Exception("Invalid profile photo format.");
                var photoPath = await _fileUploadService.UploadImageAsync(request.ProfilePhoto, IFileUploadService.UserProfilePhotosFolder);
                user.ProfilePhotoUrl = photoPath;
            }
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _fileUploadService.DeleteImage(user.ProfilePhotoUrl ?? string.Empty);
                throw new Exception($"User registration failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, user.Role.ToString());
            await SendStatusEmailAsync(user, "Registration Success", "You succesfully Registered to Gymunity!");

            _ = Task.Run(() => NewUserRegisteredAsync?.Invoke(user.Id, user.FullName, user.Email, user.Role));
            var token = await CreateTokenAsync(user, userManager);

            return PrepareAuthResponseAsync(user, token);
        }
        public async Task<string> CreateTokenAsync(AppUser user, UserManager<AppUser> userManager)
        {
            // Private Claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.UserName),
            };

            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Any())
            {
                foreach (var role in userRoles)
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // set Security Key
            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:AuthKey"] ?? string.Empty));

            // Generate JWT token
            var token = new JwtSecurityToken
                (
                    // Registered Claims
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"] ?? "0")),
                    // private claims
                    claims: claims,
                    // security Algorithem 
                    signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}