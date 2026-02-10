using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DTOs.Account.OTP;
using Gymunity.Application.DTOs.Email;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gymunity.Infrastructure.Services.Identity
{
    /// <summary>
    /// Implementation of OTP service using in-memory cache for OTP storage
    /// OTPs expire after 5 minutes and have a maximum of 3 attempts
    /// </summary>
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;
        private readonly ILogger<OtpService> _logger;
        private readonly IConfiguration _configuration;

        // OTP configuration
        private readonly TimeSpan _otpExpiry = TimeSpan.FromMinutes(5);
        private readonly int _maxAttempts = 3;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public OtpService(
            IMemoryCache cache,
            IEmailService emailService,
            ILogger<OtpService> logger,
            IConfiguration configuration)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Get email settings from configuration
            _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@gymunity.com";
            _fromName = _configuration["EmailSettings:FromName"] ?? "Gymunity";
        }

        /// <summary>
        /// Generates a 6-digit OTP, stores it in cache, and sends it via email
        /// </summary>
        public async Task<OtpResponse> GenerateAndSendOtpAsync(string email, string purpose)
        {
            try
            {
                _logger.LogInformation("Generating OTP for {Email} - Purpose: {Purpose}", email, purpose);

                // Validate inputs
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email is required", nameof(email));

                if (string.IsNullOrWhiteSpace(purpose))
                    throw new ArgumentException("Purpose is required", nameof(purpose));

                // Generate 6-digit OTP
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString(); // 6 digits
                var expiresAt = DateTime.UtcNow.Add(_otpExpiry);

                _logger.LogDebug("Generated OTP: {OtpCode} for {Email}, expires at {ExpiresAt}",
                    otpCode, email, expiresAt);

                // Create OTP record
                var otpRecord = new OtpRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = email.Trim().ToLower(),
                    OtpCode = otpCode,
                    Purpose = purpose.Trim().ToLower(),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsUsed = false,
                    Attempts = 0
                };

                // Store in cache with composite key
                var cacheKey = GetCacheKey(email, purpose);
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = expiresAt.AddMinutes(1) // Keep slightly longer for cleanup
                };

                _cache.Set(cacheKey, otpRecord, cacheOptions);

                // Send OTP via email
                await SendOtpEmailAsync(email, otpCode, purpose);

                _logger.LogInformation("OTP generated and sent successfully for {Email} - Purpose: {Purpose}",
                    email, purpose);

                return new OtpResponse
                {
                    Success = true,
                    Message = "OTP sent successfully to your email.",
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate and send OTP for {Email} - Purpose: {Purpose}",
                    email, purpose);

                return new OtpResponse
                {
                    Success = false,
                    Message = "Failed to send OTP. Please try again later."
                };
            }
        }

        /// <summary>
        /// Verifies an OTP for the given email and purpose
        /// </summary>
        public OtpResponse VerifyOtp(string email, string otpCode, string purpose)
        {
            try
            {
                _logger.LogInformation("Verifying OTP for {Email} - Purpose: {Purpose}", email, purpose);

                // Validate inputs
                if (string.IsNullOrWhiteSpace(email))
                    return new OtpResponse
                    {
                        Success = false,
                        Message = "Email is required."
                    };

                if (string.IsNullOrWhiteSpace(otpCode) || otpCode.Length != 6)
                    return new OtpResponse
                    {
                        Success = false,
                        Message = "Valid 6-digit OTP code is required."
                    };

                if (string.IsNullOrWhiteSpace(purpose))
                    return new OtpResponse
                    {
                        Success = false,
                        Message = "OTP purpose is required."
                    };

                var cacheKey = GetCacheKey(email, purpose);

                // Check if OTP exists in cache
                if (!_cache.TryGetValue<OtpRecord>(cacheKey, out var otpRecord) || otpRecord == null)
                {
                    _logger.LogWarning("OTP not found for {Email} - Purpose: {Purpose}", email, purpose);
                    return new OtpResponse
                    {
                        Success = false,
                        Message = "OTP not found or expired. Please request a new one."
                    };
                }

                // Check if OTP is already used
                if (otpRecord.IsUsed)
                {
                    _logger.LogWarning("OTP already used for {Email} - Purpose: {Purpose}", email, purpose);
                    return new OtpResponse
                    {
                        Success = false,
                        Message = "OTP already used. Please request a new one."
                    };
                }

                // Check if OTP is expired
                if (otpRecord.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("OTP expired for {Email} - Purpose: {Purpose}", email, purpose);
                    _cache.Remove(cacheKey); // Clean up expired OTP
                    return new OtpResponse
                    {
                        Success = false,
                        Message = "OTP expired. Please request a new one."
                    };
                }

                // Check maximum attempts
                if (otpRecord.Attempts >= _maxAttempts)
                {
                    _logger.LogWarning("Max attempts reached for OTP - {Email} - Purpose: {Purpose}",
                        email, purpose);
                    _cache.Remove(cacheKey); // Invalidate OTP after max attempts
                    return new OtpResponse
                    {
                        Success = false,
                        Message = "Too many attempts. OTP invalidated. Please request a new one."
                    };
                }

                // Increment attempts
                otpRecord.Attempts++;
                _cache.Set(cacheKey, otpRecord);

                // Verify OTP code
                if (otpRecord.OtpCode != otpCode)
                {
                    var attemptsRemaining = _maxAttempts - otpRecord.Attempts;
                    var message = attemptsRemaining > 0
                        ? $"Invalid OTP. {attemptsRemaining} attempt(s) remaining."
                        : "Invalid OTP. No attempts remaining.";

                    _logger.LogWarning("Invalid OTP for {Email} - Purpose: {Purpose}. Attempts: {Attempts}",
                        email, purpose, otpRecord.Attempts);

                    return new OtpResponse
                    {
                        Success = false,
                        Message = message
                    };
                }

                // OTP verified successfully - mark as used
                otpRecord.IsUsed = true;
                otpRecord.UsedAt = DateTime.UtcNow;
                _cache.Set(cacheKey, otpRecord);

                _logger.LogInformation("OTP verified successfully for {Email} - Purpose: {Purpose}",
                    email, purpose);

                return new OtpResponse
                {
                    Success = true,
                    Message = "OTP verified successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for {Email} - Purpose: {Purpose}", email, purpose);

                return new OtpResponse
                {
                    Success = false,
                    Message = "An error occurred while verifying OTP. Please try again."
                };
            }
        }

        /// <summary>
        /// Validates if an OTP is still valid (not expired and not used)
        /// </summary>
        public Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose)
        {
            try
            {
                var cacheKey = GetCacheKey(email, purpose);

                if (!_cache.TryGetValue<OtpRecord>(cacheKey, out var otpRecord) || otpRecord == null)
                    return Task.FromResult(false);

                var isValid = otpRecord.OtpCode == otpCode &&
                             !otpRecord.IsUsed &&
                             otpRecord.ExpiresAt >= DateTime.UtcNow;

                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OTP for {Email}", email);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Checks if an OTP exists for the given email and purpose
        /// </summary>
        public Task<bool> HasOtpAsync(string email, string purpose)
        {
            var cacheKey = GetCacheKey(email, purpose);
            var exists = _cache.TryGetValue<OtpRecord>(cacheKey, out _);
            return Task.FromResult(exists);
        }

        /// <summary>
        /// Gets OTP expiration time
        /// </summary>
        public Task<DateTime?> GetOtpExpirationAsync(string email, string purpose)
        {
            var cacheKey = GetCacheKey(email, purpose);

            if (_cache.TryGetValue<OtpRecord>(cacheKey, out var otpRecord) && otpRecord != null)
                return Task.FromResult<DateTime?>(otpRecord.ExpiresAt);

            return Task.FromResult<DateTime?>(null);
        }

        /// <summary>
        /// Sends OTP via email using the email service
        /// </summary>
        private async Task SendOtpEmailAsync(string email, string otpCode, string purpose)
        {
            try
            {
                var subject = GetOtpEmailSubject(purpose);
                var htmlContent = GetOtpEmailTemplate(email, otpCode, purpose);

                var emailRequest = new EmailRequest
                {
                    ToEmail = email,
                    ToName = email.Split('@')[0], // Use email username as name
                    Subject = subject,
                    Body = htmlContent,
                    IsHtml = true,
                    FromEmail = _fromEmail,
                    FromName = _fromName
                };

                await _emailService.SendEmailAsync(emailRequest);

                _logger.LogDebug("OTP email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", email);
                throw; // Re-throw to be handled by calling method
            }
        }

        /// <summary>
        /// Generates email subject based on OTP purpose
        /// </summary>
        private static string GetOtpEmailSubject(string purpose)
        {
            return purpose.ToLower() switch
            {
                "register" => "Verify Your Gymunity Account Registration",
                "login" => "Your Gymunity Login Verification Code",
                "reset-password" => "Reset Your Gymunity Account Password",
                "change-email" => "Confirm Your Email Change",
                _ => "Your Gymunity Verification Code"
            };
        }

        /// <summary>
        /// Generates HTML email template for OTP
        /// </summary>
        private static string GetOtpEmailTemplate(string email, string otpCode, string purpose)
        {
            var purposeText = purpose.ToLower() switch
            {
                "register" => "complete your account registration",
                "login" => "log in to your account",
                "reset-password" => "reset your password",
                "change-email" => "confirm your email change",
                _ => "complete your verification"
            };

            var actionButtonText = purpose.ToLower() switch
            {
                "register" => "Complete Registration",
                "login" => "Continue Login",
                "reset-password" => "Reset Password",
                "change-email" => "Confirm Change",
                _ => "Verify Now"
            };

            return $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>Gymunity Verification</title>
                    <style>
                        body {
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            margin: 0;
                            padding: 0;
                            background-color: #f8f9fa;
                        }
                        .container {
                            max-width: 600px;
                            margin: 20px auto;
                            background-color: #ffffff;
                            border-radius: 12px;
                            overflow: hidden;
                            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
                        }
                        .header {
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            padding: 40px 30px;
                            text-align: center;
                            color: white;
                        }
                        .logo {
                            font-size: 36px;
                            font-weight: bold;
                            margin-bottom: 10px;
                        }
                        .content {
                            padding: 40px 30px;
                            text-align: center;
                            color: #333333;
                            line-height: 1.6;
                        }
                        .otp-code {
                            font-size: 48px;
                            font-weight: bold;
                            letter-spacing: 15px;
                            color: #667eea;
                            margin: 30px 0;
                            padding: 20px;
                            background: #f8f9fa;
                            border-radius: 10px;
                            display: inline-block;
                        }
                        .button {
                            display: inline-block;
                            padding: 15px 40px;
                            margin-top: 30px;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            color: white !important;
                            text-decoration: none;
                            border-radius: 50px;
                            font-weight: bold;
                            font-size: 16px;
                            border: none;
                            cursor: pointer;
                        }
                        .info-box {
                            background-color: #e8f4fd;
                            border-left: 4px solid #2196f3;
                            padding: 20px;
                            margin: 30px 0;
                            text-align: left;
                            border-radius: 5px;
                        }
                        .footer {
                            background-color: #1a1a1a;
                            color: #888888;
                            padding: 30px;
                            text-align: center;
                            font-size: 14px;
                        }
                        .expiry-notice {
                            color: #ff6b6b;
                            font-weight: bold;
                            margin: 20px 0;
                        }
                    </style>
                </head>
                <body>
                    <div class="container">
                        <div class="header">
                            <div class="logo">GYMUNITY</div>
                            <h1>Security Verification</h1>
                        </div>
                        
                        <div class="content">
                            <h2>Your Verification Code</h2>
                            <p>Hello,</p>
                            <p>Use the verification code below to {{purposeText}}.</p>
                            
                            <div class="otp-code">{{otpCode}}</div>
                            
                            <div class="expiry-notice">
                                ⏰ This code will expire in 5 minutes
                            </div>
                            
                            <div class="info-box">
                                <strong>🔒 Security Notice:</strong><br><br>
                                • Never share this code with anyone<br>
                                • Gymunity will never ask for your OTP via phone or email<br>
                                • This code is valid for one-time use only<br>
                                • If you didn't request this, please ignore this email
                            </div>
                            
                            <a href="#" class="button">{{actionButtonText}}</a>
                            
                            <p style="margin-top: 30px; color: #666;">
                                Having trouble? <a href="mailto:support@gymunity.com" style="color: #667eea;">Contact Support</a>
                            </p>
                        </div>
                        
                        <div class="footer">
                            <p>&copy; {{DateTime.Now.Year}} Gymunity Inc. All rights reserved.</p>
                            <p>This is an automated security message. Please do not reply.</p>
                            <p style="margin-top: 10px; font-size: 12px;">
                                For security reasons, this email was sent to {{email}}
                            </p>
                        </div>
                    </div>
                </body>
                </html>
                """;
        }

        /// <summary>
        /// Generates cache key for OTP storage
        /// </summary>
        private static string GetCacheKey(string email, string purpose)
        {
            var normalizedEmail = email.Trim().ToLower();
            var normalizedPurpose = purpose.Trim().ToLower();
            return $"otp:{normalizedEmail}:{normalizedPurpose}";
        }
    }
}
//public class OtpService(
//    IMemoryCache cache,
//    IEmailService emailService,
//    ILogger<OtpService> logger,
//    IConfiguration configuration) : IOtpService
//{
//    private readonly IMemoryCache _cache = cache;
//    private readonly IEmailService _emailService = emailService;
//    private readonly ILogger<OtpService> _logger = logger;
//    private readonly TimeSpan _otpExpiry = TimeSpan.FromMinutes(5);
//    private readonly IConfiguration _configuration = configuration;

//    public async Task<OtpResponse> GenerateAndSendOtpAsync(string email, string purpose)
//    {
//        try
//        {
//            // Generate 6-digit OTP
//            var random = new Random();
//            var otpCode = random.Next(100000, 999999).ToString();

//            // Expiration (5 minutes)
//            var expiresAt = DateTime.UtcNow.Add(_otpExpiry);

//            // Store in cache with email-purpose as key
//            var cacheKey = GetCacheKey(email, purpose);
//            var otpRecord = new OtpRecord
//            {
//                OtpCode = otpCode,
//                Email = email,
//                Purpose = purpose,
//                CreatedAt = DateTime.UtcNow,
//                ExpiresAt = expiresAt,
//                IsUsed = false,
//                Attempts = 0
//            };

//            _cache.Set(cacheKey, otpRecord, _otpExpiry.Add(TimeSpan.FromMinutes(1)));

//            // Send OTP via email
//            await SendOtpEmailAsync(email, otpCode, purpose);

//            _logger.LogInformation("OTP generated for {Email} - Purpose: {Purpose}", email, purpose);

//            return new OtpResponse
//            {
//                Success = true,
//                Message = "OTP sent successfully",
//                ExpiresAt = expiresAt
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to generate OTP for {Email}", email);
//            return new OtpResponse
//            {
//                Success = false,
//                Message = "Failed to send OTP. Please try again."
//            };
//        }
//    }

//    public async Task<OtpResponse> VerifyOtpAsync(string email, string otpCode, string purpose)
//    {
//        var cacheKey = GetCacheKey(email, purpose);

//        if (!_cache.TryGetValue<OtpRecord>(cacheKey, out var otpRecord))
//        {
//            return new OtpResponse
//            {
//                Success = false,
//                Message = "OTP not found or expired. Please request a new one."
//            };
//        }

//        if (otpRecord!.IsUsed)
//        {
//            return new OtpResponse
//            {
//                Success = false,
//                Message = "OTP already used. Please request a new one."
//            };
//        }

//        if (otpRecord.ExpiresAt < DateTime.UtcNow)
//        {
//            _cache.Remove(cacheKey);
//            return new OtpResponse
//            {
//                Success = false,
//                Message = "OTP expired. Please request a new one."
//            };
//        }

//        if (otpRecord.Attempts >= 3)
//        {
//            _cache.Remove(cacheKey);
//            return new OtpResponse
//            {
//                Success = false,
//                Message = "Too many attempts. OTP invalidated. Request a new one."
//            };
//        }

//        // Increment attempts
//        otpRecord.Attempts++;
//        _cache.Set(cacheKey, otpRecord);

//        if (otpRecord.OtpCode != otpCode)
//        {
//            return new OtpResponse
//            {
//                Success = false,
//                Message = $"Invalid OTP. {3 - otpRecord.Attempts} attempts remaining."
//            };
//        }

//        // OTP verified successfully - remove from cache
//        _cache.Remove(cacheKey);

//        _logger.LogInformation("OTP verified successfully for {Email} - Purpose: {Purpose}", email, purpose);

//        return new OtpResponse
//        {
//            Success = true,
//            Message = "OTP verified successfully"
//        };
//    }

//    public Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose)
//    {
//        var cacheKey = GetCacheKey(email, purpose);

//        if (!_cache.TryGetValue<OtpRecord>(cacheKey, out var otpRecord))
//            return Task.FromResult(false);

//        var isValid = otpRecord != null &&
//               !otpRecord.IsUsed &&
//                otpRecord.OtpCode == otpCode &&
//                otpRecord.ExpiresAt >= DateTime.UtcNow;

//        return Task.FromResult(isValid);
//    }

//    public static Task CleanupExpiredOtpsAsync()
//    {
//        // Cache auto-expires based on sliding expiration
//        return Task.CompletedTask;
//    }

//    private async Task SendOtpEmailAsync(string email, string otpCode, string purpose)
//    {
//        var subject = purpose switch
//        {
//            "login" => "Your Gymunity Login OTP",
//            "register" => "Verify Your Gymunity Account",
//            "reset-password" => "Reset Your Password",
//            "change-email" => "Confirm Email Change",
//            _ => "Your Gymunity Verification Code"
//        };

//        var htmlContent = GetOtpEmailTemplate(email, otpCode, purpose);

//        await _emailService.SendEmailAsync(email, subject, htmlContent);
//    }

//    private static string GetOtpEmailTemplate(string email, string otpCode, string purpose)
//    {
//        var purposeText = purpose switch
//        {
//            "login" => "login to your account",
//            "register" => "verify your account registration",
//            "reset-password" => "reset your password",
//            "change-email" => "confirm your email change",
//            _ => "complete your verification"
//        };

//        return $$"""
//        <!DOCTYPE html>
//        <html>
//        <head>
//            <style>
//                body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }
//                .container { max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; margin-top: 20px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }
//                .header { background: linear-gradient(135deg, #FF4B2B 0%, #FF416C 100%); padding: 30px 20px; text-align: center; color: white; }
//                .content { padding: 30px; text-align: center; color: #333333; line-height: 1.6; }
//                .otp-code { font-size: 42px; font-weight: bold; letter-spacing: 10px; color: #FF416C; margin: 25px 0; background: #f9f9f9; padding: 20px; border-radius: 10px; display: inline-block; }
//                .footer { background-color: #1a1a1a; color: #888888; padding: 20px; text-align: center; font-size: 12px; }
//                .warning { background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; font-size: 14px; }
//            </style>
//        </head>
//        <body>
//            <div class="container">
//                <div class="header">
//                    <h1>GYMUNITY</h1>
//                    <p>Security Verification</p>
//                </div>
//                <div class="content">
//                    <h2>Your Verification Code</h2>
//                    <p>Hello,</p>
//                    <p>Use this OTP to {{purposeText}}:</p>

//                    <div class="otp-code">{{otpCode}}</div>

//                    <p>This code will expire in <strong>5 minutes</strong>.</p>

//                    <div class="warning">
//                        <strong>⚠️ Security Notice:</strong><br>
//                        • Never share this code with anyone<br>
//                        • Gymunity will never ask for your OTP<br>
//                        • If you didn't request this, please ignore this email
//                    </div>

//                    <p>Need help? <a href="mailto:support@gymunity.com">Contact Support</a></p>
//                </div>
//                <div class="footer">
//                    <p>&copy; 2026 Gymunity Inc. All Rights Reserved.<br>
//                    This is an automated security message from Gymunity.</p>
//                </div>
//            </div>
//        </body>
//        </html>
//        """;
//    }
//    private static string GetCacheKey(string email, string purpose) => $"otp:{email}:{purpose}";
//}