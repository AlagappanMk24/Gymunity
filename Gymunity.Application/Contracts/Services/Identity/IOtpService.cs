using Gymunity.Application.DTOs.Account.OTP;

namespace Gymunity.Application.Contracts.Services.Identity
{
    /// <summary>
    /// Interface for OTP (One-Time Password) service
    /// Provides methods for generating, sending, and verifying OTPs
    /// </summary>
    public interface IOtpService
    {
        /// <summary>
        /// Generates and sends an OTP to the specified email for the given purpose
        /// </summary>
        /// <param name="email">Recipient's email address</param>
        /// <param name="purpose">Purpose of OTP (register, login, reset-password, change-email)</param>
        /// <returns>OTP result with success status and message</returns>
        Task<OtpResponse> GenerateAndSendOtpAsync(string email, string purpose);

        /// <summary>
        /// Verifies an OTP for the specified email and purpose
        /// </summary>
        /// <param name="email">Email address to verify</param>
        /// <param name="otpCode">6-digit OTP code</param>
        /// <param name="purpose">Purpose of OTP</param>
        /// <returns>OTP result with verification status</returns>
        OtpResponse VerifyOtp(string email, string otpCode, string purpose);

        /// <summary>
        /// Validates if an OTP is still valid (not expired and not used)
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="otpCode">OTP code to validate</param>
        /// <param name="purpose">Purpose of OTP</param>
        /// <returns>True if OTP is valid, false otherwise</returns>
        Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose);

        /// <summary>
        /// Checks if an OTP exists for the given email and purpose
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="purpose">Purpose of OTP</param>
        /// <returns>True if OTP exists, false otherwise</returns>
        Task<bool> HasOtpAsync(string email, string purpose);

        /// <summary>
        /// Gets OTP expiration time for the given email and purpose
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="purpose">Purpose of OTP</param>
        /// <returns>Expiration DateTime if OTP exists, null otherwise</returns>
        Task<DateTime?> GetOtpExpirationAsync(string email, string purpose);
    }
}