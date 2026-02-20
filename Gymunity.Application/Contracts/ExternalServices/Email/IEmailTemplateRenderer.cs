namespace Gymunity.Application.Contracts.ExternalServices.Email
{
    /// <summary>
    /// Service for generating HTML email templates
    /// Separates template logic from email sending logic
    /// </summary>
    public interface IEmailTemplateRenderer
    {
        /// <summary>
        /// Login Confirmation Email Template
        /// </summary>
        string GetLoginConfirmationEmail(string userName, string loginTime, string location, string device);

        /// <summary>
        /// Registration Confirmation Email Template
        /// </summary>
        string GetRegistrationConfirmationEmail(string userName);

        /// <summary>
        /// OTP Verification Email Template
        /// </summary>
        string GetOtpVerificationEmail(string otpCode, string purpose);

        /// <summary>
        /// Reset Password Link Email Template
        /// </summary>
        string GetResetPasswordLinkEmail(string userName, string resetLink);

        /// <summary>
        /// Reset Password Confirmation Email Template
        /// </summary>
        string GetResetPasswordConfirmationEmail(string userName);

        /// <summary>
        /// Change Password Confirmation Email Template
        /// </summary>
        string GetChangePasswordConfirmationEmail(string userName, string changeDate, string changeTime, string device);

        /// <summary>
        /// Generic email template with custom content
        /// </summary>
        string GetCustomEmailTemplate(string title, string message, string buttonText = null, string buttonLink = null);
    }
}