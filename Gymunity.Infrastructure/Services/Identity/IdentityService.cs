using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DTOs.Account;
using Gymunity.Application.DTOs.Account.OTP;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
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
        IOtpService otpService,
        IConfiguration configuration,
        IImageUrlResolver imageUrlResolver,
        ILogger<IdentityService> logger) : BaseIdentityService(emailService, imageUrlResolver, userManager), IIdentityService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly IFileUploadService _fileUploadService = fileUploadService;
        private readonly IOtpService _otpService = otpService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<IdentityService> _logger = logger;

        // ✅ Observable events for notification handlers
        public event Func<string, string, string, UserRole, Task>? NewUserRegisteredAsync;

        // ================= REGISTRATION FLOW =================
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Starting registration for email: {Email}", request.Email);

                // Step 1: Check if OTP is provided and verified
                if (!string.IsNullOrEmpty(request.OtpCode))
                {
                    _logger.LogInformation("Verifying OTP for registration: {Email}", request.Email);

                    var otpResult = _otpService.VerifyOtp(request.Email, request.OtpCode, "register");
                    if (!otpResult.Success)
                    {
                        _logger.LogWarning("OTP verification failed for {Email}: {Message}",
                            request.Email, otpResult.Message);
                        throw new Exception($"OTP verification failed: {otpResult.Message}");
                    }

                    request.IsOtpVerified = true;
                    _logger.LogInformation("OTP verified successfully for {Email}", request.Email);
                }

                // Step 2: If no OTP provided or not verified, send OTP
                if (!request.IsOtpVerified)
                {
                    _logger.LogInformation("Sending OTP for registration to {Email}", request.Email);

                    // Check if email already exists (but don't create account yet)
                    if (await IsEmailUniqueAsync(request.Email) == false)
                        throw new Exception("Email is already registered.");

                    if (await IsUserNameUniqueAsync(request.UserName) == false)
                        throw new Exception("Username is already taken.");

                    // Send OTP
                    var otpResult = await _otpService.GenerateAndSendOtpAsync(request.Email, "register");
                    if (!otpResult.Success)
                        throw new Exception("Failed to send OTP. Please try again.");

                    _logger.LogInformation("OTP sent for registration to {Email}", request.Email);

                    // Return response indicating OTP required
                    return new AuthResponse
                    {
                        Id = string.Empty,
                        Name = string.Empty,
                        UserName = string.Empty,
                        Email = request.Email,
                        Role = (UserRole)request.Role,
                        RequiresOtp = true,
                        Message = "OTP sent to your email. Please verify to complete registration.",
                        OtpExpiresAt = otpResult.ExpiresAt,
                        IsAccountActive = false
                    };
                }

                // Step 3: OTP verified - Create user account
                _logger.LogInformation("Creating user account for {Email} after OTP verification", request.Email);

                // Final validation checks
                if (!await IsEmailUniqueAsync(request.Email))
                    throw new Exception("Email is already registered.");

                if (!await IsUserNameUniqueAsync(request.UserName))
                    throw new Exception("Username is already taken.");

                var user = new AppUser
                {
                    UserName = request.UserName.ToLower(),
                    Email = request.Email,
                    FullName = request.FullName,
                    Role = (UserRole)request.Role,
                };

                // Upload profile photo
                if (request.ProfilePhoto != null)
                {
                    if (!_fileUploadService.IsValidImageFile(request.ProfilePhoto))
                        throw new Exception("Invalid profile photo format.");

                    var photoPath = await _fileUploadService.UploadImageAsync(
                        request.ProfilePhoto,
                        IFileUploadService.UserProfilePhotosFolder);
                    user.ProfilePhotoUrl = photoPath;
                }

                // Create user
                var result = await _userManager.CreateAsync(user, request.Password);
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

                // Send welcome email
                string welcomeBody = GetWelcomeEmailTemplate(user.FullName ?? user.UserName);
                await SendStatusEmailAsync(user, "Registration Success", welcomeBody);

                // Notify admin
                _ = Task.Run(() => NewUserRegisteredAsync?.Invoke(
                    user.Id, user.FullName, user.Email, user.Role));

                // Generate token for immediate login
                var token = await CreateTokenAsync(user);

                _logger.LogInformation("User registration completed successfully for {Email}", request.Email);

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
                _logger.LogError(ex, "Registration failed for {Email}", request.Email);
                throw;
            }
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
                await SendStatusEmailAsync(user, "Login Success", "You successfully signed in to Gymunity!");

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

        // ================= OTP HELPER METHODS =================
        public async Task<OtpResponse> SendRegistrationOtpAsync(string email)
        {
            try
            {
                _logger.LogInformation("Sending registration OTP to {Email}", email);

                // Check if email already exists
                if (await IsEmailUniqueAsync(email) == false)
                    throw new Exception("Email is already registered.");

                var result = await _otpService.GenerateAndSendOtpAsync(email, "register");

                if (!result.Success)
                    throw new Exception("Failed to send OTP. Please try again.");

                _logger.LogInformation("Registration OTP sent to {Email}", email);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send registration OTP to {Email}", email);
                throw;
            }
        }

        public OtpResponse VerifyRegistrationOtp(string email, string otpCode)
        {
            return  _otpService.VerifyOtp(email, otpCode, "register");
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

        public async Task<AuthResponse> CompleteRegistrationWithOtpAsync(CompleteRegistrationRequest request)
        {
            try
            {
                _logger.LogInformation("Completing registration with OTP for {Email}", request.Email);

                // Verify OTP first
                var otpResult = _otpService.VerifyOtp(request.Email, request.OtpCode, "register");
                if (!otpResult.Success)
                    throw new Exception($"OTP verification failed: {otpResult.Message}");

                // Set OTP as verified in registration data
                request.RegistrationData.OtpCode = request.OtpCode;
                request.RegistrationData.IsOtpVerified = true;

                // Complete registration
                return await RegisterAsync(request.RegistrationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to complete registration with OTP for {Email}", request.Email);
                throw;
            }
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
                _configuration["JWT:AuthKey"] ?? throw new Exception("JWT AuthKey not configured")));

            // Generate JWT token
            var token = new JwtSecurityToken
            (
                // Registered Claims
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"] ?? "10")),
                // private claims
                claims: claims,
                // security Algorithm 
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private static string GetWelcomeEmailTemplate(string userName)
        {
            string html = $$"""
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <style>
                        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }
                        .container { max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; margin-top: 20px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }
                        .header { background: linear-gradient(135deg, #FF4B2B 0%, #FF416C 100%); padding: 40px 20px; text-align: center; color: white; }
                        .header h1 { margin: 0; font-size: 28px; letter-spacing: 2px; text-transform: uppercase; }
                        .content { padding: 30px; text-align: center; color: #333333; line-height: 1.6; }
                        .welcome-text { font-size: 18px; font-weight: bold; color: #FF416C; }
                        .button { display: inline-block; padding: 15px 30px; margin-top: 25px; background-color: #FF416C; color: #ffffff !important; text-decoration: none; border-radius: 50px; font-weight: bold; }
                        .footer { background-color: #1a1a1a; color: #888888; padding: 20px; text-align: center; font-size: 12px; }
                        .features { display: flex; justify-content: space-around; margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px; }
                        .feature-item { flex: 1; font-size: 13px; }
                    </style>
                </head>
                <body>
                    <div class="container">
                        <div class="header">
                            <h1>GYMUNITY</h1>
                            <p>Where Fitness Meets Community</p>
                        </div>
                        <div class="content">
                            <p class="welcome-text">Hi {{userName}},</p>
                            <p>Welcome to the family! We are thrilled to have you join <strong>Gymunity</strong>. Your journey to a stronger, healthier version of yourself starts right here, right now.</p>
                            <a href="https://gymunity.com/login" class="button">START YOUR WORKOUT</a>
                            <div class="features">
                                <div class="feature-item"><strong>💪 Train</strong><br>Expert Plans</div>
                                <div class="feature-item"><strong>🤝 Connect</strong><br>Top Trainers</div>
                                <div class="feature-item"><strong>📈 Track</strong><br>Real Progress</div>
                            </div>
                        </div>
                        <div class="footer">
                            <p>&copy; 2026 Gymunity Inc. All Rights Reserved.<br>
                            You received this email because you signed up at Gymunity.com</p>
                        </div>
                    </div>
                </body>
                </html>
                """;
            return html;
        }
    }
}