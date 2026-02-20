using Gymunity.Application.DTOs.Email;

namespace Gymunity.Application.Contracts.ExternalServices.Email
{
    public interface IEmailService
    {
        /// <summary>
        /// Send a single email
        /// </summary>
        Task SendEmailAsync(EmailRequest request);

        /// <summary>
        /// Send bulk emails
        /// </summary>
        Task SendBulkEmailAsync(List<EmailRequest> requests);

        /// <summary>
        /// Send email using template (convenience method)
        /// </summary>
        Task SendTemplatedEmailAsync(string toEmail, string toName, string subject, string template,
            Dictionary<string, string> placeholders = null);
    }
}