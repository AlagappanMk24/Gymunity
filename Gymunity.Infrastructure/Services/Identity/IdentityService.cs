using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DTOs;
using Gymunity.Application.DTOs.Account.OTP;
using Gymunity.Application.DTOs.Auth;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Enums;
using Gymunity.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Gymunity.Infrastructure.Services.Identity
{
    public class IdentityService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager, 
        IFileUploadService fileUploadService,
        IEmailService emailService,
        IEmailTemplateRenderer emailTemplateRenderer,
        IUserInfoService userInfoService,
        IOtpService otpService,
        IConfiguration configuration,
        IImageUrlResolver imageUrlResolver,
        IMemoryCache cache,
        ILogger<IdentityService> logger) : BaseIdentityService(emailService, imageUrlResolver, userManager), IIdentityService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly IFileUploadService _fileUploadService = fileUploadService;
        private readonly IEmailTemplateRenderer _emailTemplateRenderer = emailTemplateRenderer;
        private readonly IUserInfoService _userInfoService = userInfoService;
        private readonly IOtpService _otpService = otpService;
        private readonly IMemoryCache _cache = cache;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<IdentityService> _logger = logger;

        // Cache keys
        private const string RegistrationCachePrefix = "registration_";
        private const int RegistrationCacheDurationMinutes = 10; // Registration data valid for 10 minutes

        // ✅ Observable events for notification handlers
        public event Func<string, string, string, UserRole, Task>? NewUserRegisteredAsync;

        // ================= REGISTRATION FLOW =================

        /// <summary>
        /// Step 1: Initiate registration - validate data and send OTP
        /// </summary>
        public async Task<InitiateRegistrationResponse> InitiateRegistrationAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Initiating registration for email: {Email}", request.Email);

                // Step 1: Validate basic inputs
                if (string.IsNullOrWhiteSpace(request.Email))
                    throw new Exception("Email is required.");

                if (string.IsNullOrWhiteSpace(request.UserName))
                    throw new Exception("Username is required.");

                // Step 2: Check if email/username already exists
                if (await IsEmailUniqueAsync(request.Email) == false)
                    throw new Exception("Email is already registered.");

                if (await IsUserNameUniqueAsync(request.UserName) == false)
                    throw new Exception("Username is already taken.");

                // Step 3: Validate password strength
                if (request.Password != request.ConfirmPassword)
                    throw new Exception("Passwords do not match.");

                // Step 4: Store registration data in cache
                var cacheKey = GetRegistrationCacheKey(request.Email);

                // Create a cache-friendly object (without IFormFile)
                var cachedData = new CachedRegistrationData
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    Password = request.Password,
                    FullName = request.FullName,
                    Role = request.Role,
                    // Convert IFormFile to byte array
                    ProfilePhotoBytes = await ConvertFormFileToByteArrayAsync(request.ProfilePhoto),
                    ProfilePhotoFileName = request.ProfilePhoto?.FileName,
                    ProfilePhotoContentType = request.ProfilePhoto?.ContentType
                };

                _cache.Set(cacheKey, cachedData, TimeSpan.FromMinutes(RegistrationCacheDurationMinutes));

                _logger.LogInformation("Registration data cached for {Email} with key {CacheKey}",
                    request.Email, cacheKey);

                // Step 5: Send OTP
                var otpResult = await _otpService.GenerateAndSendOtpAsync(request.Email, "register");

                if (!otpResult.Success)
                    throw new Exception("Failed to send OTP. Please try again.");

                _logger.LogInformation("OTP sent for registration to {Email}", request.Email);

                return new InitiateRegistrationResponse
                {
                    Success = true,
                    Message = "OTP sent to your email. Please verify to complete registration.",
                    OtpExpiresAt = otpResult.ExpiresAt,
                    Email = request.Email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration initiation failed for {Email}", request.Email);
                throw;
            }
        }

        /// <summary>
        /// Step 2: Complete registration with OTP verification
        /// </summary>
        public async Task<AuthResponse> CompleteRegistrationAsync(string email, string otpCode)
        {
            try
            {
                _logger.LogInformation("Completing registration for {Email} with OTP", email);

                // Step 1: Verify OTP
                var otpResult = _otpService.VerifyOtp(email, otpCode, "register");
                if (!otpResult.Success)
                    throw new Exception($"OTP verification failed: {otpResult.Message}");

                _logger.LogInformation("OTP verified successfully for {Email}", email);

                // Step 2: Retrieve cached registration data
                var cacheKey = GetRegistrationCacheKey(email);
                if (!_cache.TryGetValue<CachedRegistrationData>(cacheKey, out var cachedData))
                {
                    throw new Exception("Registration session expired. Please start the registration process again.");
                }

                // Step 3: Final validation (double-check uniqueness)
                if (!await IsEmailUniqueAsync(email))
                    throw new Exception("Email is already registered.");

                if (!await IsUserNameUniqueAsync(cachedData.UserName))
                    throw new Exception("Username is already taken.");

                // Step 4: Create user account
                var user = await CreateUserFromCachedDataAsync(cachedData);

                // Step 5: Send welcome email
                //await SendStatusEmailAsync(user, "Registration Success", welcomeBody);

                await SendStatusEmailAsync(user, "🎉 Welcome to Gymunity!",
                   _emailTemplateRenderer.GetRegistrationConfirmationEmail(user.UserName));

                // Step 6: Notify admin
                _ = Task.Run(() => NewUserRegisteredAsync?.Invoke(
                    user.Id, user.FullName, user.Email, user.Role));

                // Step 7: Generate token for immediate login
                var token = await CreateTokenAsync(user);

                // Step 8: Clear cached data
                _cache.Remove(cacheKey);

                _logger.LogInformation("Registration completed successfully for {Email}", email);

                return new AuthResponse
                {
                    Id = user.Id,
                    Name = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    Token = token,
                    RequiresOtp = false,
                    Message = "Registration successful!",
                    IsAccountActive = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration completion failed for {Email}", email);
                throw;
            }
        }


        /// <summary>
        /// Resend OTP for registration
        /// </summary>
        public async Task<OtpResponse> ResendRegistrationOtpAsync(string email)
        {
            try
            {
                _logger.LogInformation("Resending registration OTP to {Email}", email);

                // Check if registration data exists in cache
                var cacheKey = GetRegistrationCacheKey(email);
                if (!_cache.TryGetValue<CachedRegistrationData>(cacheKey, out var cachedData))
                {
                    _logger.LogWarning("Registration session expired for {Email}. Cache key: {CacheKey}", email, cacheKey);
                    throw new Exception("Your registration session has expired. Registrations are valid for 10 minutes. Please start the registration process again.");
                }

                // Resend OTP
                var otpResult = await _otpService.GenerateAndSendOtpAsync(email, "register");

                if (!otpResult.Success)
                    throw new Exception("Failed to resend OTP. Please try again.");

                // Extend cache duration
                _cache.Set(cacheKey, cachedData, TimeSpan.FromMinutes(RegistrationCacheDurationMinutes));

                _logger.LogInformation("Registration OTP resent to {Email}", email);
                return otpResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend registration OTP to {Email}", email);
                throw;
            }
        }

        // ================= HELPER METHODS =================

        private static async Task<byte[]> ConvertFormFileToByteArrayAsync(IFormFile formFile)
        {
            if (formFile == null)
                return null;

            using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        private async Task<AppUser> CreateUserFromCachedDataAsync(CachedRegistrationData cachedData)
        {
            var user = new AppUser
            {
                UserName = cachedData.UserName.ToLower(),
                Email = cachedData.Email,
                FullName = cachedData.FullName,
                Role = (UserRole)cachedData.Role,
            };

            // Upload profile photo from byte array
            if (cachedData.ProfilePhotoBytes != null && cachedData.ProfilePhotoBytes.Length > 0)
            {
                // Create a temporary file or stream from byte array
                using var memoryStream = new MemoryStream(cachedData.ProfilePhotoBytes);

                // Create FormFile from byte array
                var formFile = new FormFile(
                    memoryStream,
                    0,
                    cachedData.ProfilePhotoBytes.Length,
                    "ProfilePhoto",
                    cachedData.ProfilePhotoFileName ?? "profile.jpg")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = cachedData.ProfilePhotoContentType ?? "image/jpeg"
                };

                if (!_fileUploadService.IsValidImageFile(formFile))
                    throw new Exception("Invalid profile photo format.");

                var photoPath = await _fileUploadService.UploadImageAsync(
                    formFile,
                    IFileUploadService.UserProfilePhotosFolder);
                user.ProfilePhotoUrl = photoPath;
            }

            // Create user
            var result = await _userManager.CreateAsync(user, cachedData.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                // Clean up uploaded photo if user creation failed
                if (!string.IsNullOrEmpty(user.ProfilePhotoUrl))
                    _fileUploadService.DeleteImage(user.ProfilePhotoUrl);

                throw new Exception($"User registration failed: {errors}");
            }

            // Add role
            await _userManager.AddToRoleAsync(user, user.Role.ToString());

            return user;
        }

        private static string GetRegistrationCacheKey(string email)
        {
            return $"{RegistrationCachePrefix}{email.ToLower()}";
        }

        // ================= LOGIN FLOW =================
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for: {EmailOrUserName}", request.EmailOrUserName);

                // Find user
                var user = await _userManager.FindByEmailAsync(request.EmailOrUserName)
                           ?? await _userManager.FindByNameAsync(request.EmailOrUserName)
                           ?? throw new Exception("Invalid credentials.");

                _logger.LogInformation("User found for login: {UserId}, {Email}", user.Id, user.Email);

                // Step 1: Validate password
                var passwordResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!passwordResult.Succeeded)
                {
                    _logger.LogWarning("Invalid password for user: {Email}", user.Email);
                    throw new Exception("Invalid email/username or password.");
                }

                // Step 2: Check if OTP is provided
                if (!string.IsNullOrEmpty(request.OtpCode))
                {
                    _logger.LogInformation("Verifying login OTP for {Email}", user.Email);

                    var otpResult = _otpService.VerifyOtp(user.Email, request.OtpCode, "login");
                    if (!otpResult.Success)
                    {
                        _logger.LogWarning("Login OTP verification failed for {Email}: {Message}",
                            user.Email, otpResult.Message);
                        throw new Exception($"OTP verification failed: {otpResult.Message}");
                    }

                    _logger.LogInformation("Login OTP verified for {Email}", user.Email);
                }
                else
                {
                    // Step 3: No OTP provided - Send OTP
                    _logger.LogInformation("Sending login OTP to {Email}", user.Email);

                    var otpResult = await _otpService.GenerateAndSendOtpAsync(user.Email, "login");
                    if (!otpResult.Success)
                        throw new Exception("Failed to send OTP. Please try again.");

                    _logger.LogInformation("Login OTP sent to {Email}", user.Email);

                    return new AuthResponse
                    {
                        Id = user.Id,
                        Name = user.FullName,
                        UserName = user.UserName,
                        Email = user.Email,
                        Role = user.Role,
                        ProfilePhotoUrl = user.ProfilePhotoUrl,
                        RequiresOtp = true,
                        Message = "OTP sent to your email. Please verify to complete login.",
                        OtpExpiresAt = otpResult.ExpiresAt,
                    };
                }

                // Step 4: OTP verified - Generate token
                _logger.LogInformation("Generating token for user: {Email}", user.Email);

                var token = await CreateTokenAsync(user);

                // Get real client info for login confirmation email
                var userInfo = await _userInfoService.GetClientInfoAsync();

                await SendStatusEmailAsync(user, "🔐 Login Confirmation",
                    _emailTemplateRenderer.GetLoginConfirmationEmail(
                        user.UserName,
                        DateTime.Now.ToString("HH:mm:ss"),
                        userInfo.Location,
                        userInfo.Device
                    ));

                _logger.LogInformation("Login successful for user: {Email}", user.Email);

                return new AuthResponse
                {
                    Id = user.Id,
                    Name = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    Token = token,
                    RequiresOtp = false,
                    Message = "Login successful!",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {EmailOrUserName}", request.EmailOrUserName);
                throw;
            }
        }
        public async Task<OtpResponse> SendLoginOtpAsync(string email)
        {
            try
            {
                _logger.LogInformation("Sending login OTP to {Email}", email);

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    throw new Exception("User not found.");

                var result = await _otpService.GenerateAndSendOtpAsync(email, "login");

                if (!result.Success)
                    throw new Exception("Failed to send OTP. Please try again.");

                _logger.LogInformation("Login OTP sent to {Email}", email);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send login OTP to {Email}", email);
                throw;
            }
        }

        public OtpResponse VerifyRegistrationOtp(string email, string otpCode)
        {
            return  _otpService.VerifyOtp(email, otpCode, "register");
        }

        // ================= TOKEN GENERATION =================
        public async Task<string> CreateTokenAsync(AppUser user)
        {
            // Private Claims
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserName),
            new("role", user.Role.ToString()),
            new("fullName", user.FullName ?? string.Empty)
        };

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Any())
            {
                foreach (var role in userRoles)
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Set Security Key
            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JWT:Key"] ?? throw new Exception("JWT Key not configured")));

            // Generate JWT token
            var token = new JwtSecurityToken
            (
                // Registered Claims
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"] ?? "10")),
                // private claims
                claims: claims,
                // security Algorithm 
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}