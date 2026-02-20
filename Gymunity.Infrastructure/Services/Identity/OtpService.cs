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
        private readonly IEmailTemplateRenderer _emailTemplateRenderer;
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
            IEmailTemplateRenderer emailTemplateRenderer,
            ILogger<OtpService> logger,
            IConfiguration configuration)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _emailTemplateRenderer = emailTemplateRenderer ?? throw new ArgumentNullException(nameof(emailTemplateRenderer));
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
                var htmlContent = _emailTemplateRenderer.GetOtpVerificationEmail(otpCode, purpose);

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