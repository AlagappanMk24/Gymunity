using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.DTOs.Email;
using Gymunity.Infrastructure.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Gymunity.Infrastructure.ExternalServices
{
    public class EmailService(IOptions<EmailSettings> emailOptions, ILogger<EmailService> logger) : IEmailService
    {
        private readonly EmailSettings _settings = emailOptions.Value;
        private readonly ILogger<EmailService> _logger = logger;
        public async Task SendEmailAsync(EmailRequest request)
        {
            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
                message.To.Add(new MailAddress(request.ToEmail, request.ToName));
                message.Subject = request.Subject;
                message.Body = request.Body;
                message.IsBodyHtml = request.IsHtml;

                if (!string.IsNullOrEmpty(request.CcEmail))
                    message.CC.Add(request.CcEmail);

                if (!string.IsNullOrEmpty(request.BccEmail))
                    message.Bcc.Add(request.BccEmail);

                using var smtpClient = CreateSmtpClient();
                await smtpClient.SendMailAsync(message);

                _logger.LogInformation("Email sent successfully to {Email}", request.ToEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", request.ToEmail);
                throw;
            }
        }
        public async Task SendBulkEmailAsync(List<EmailRequest> requests)
        {
            // Sending emails in parallel
            var tasks = requests.Select(SendEmailAsync);
            await Task.WhenAll(tasks);
        }
        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.FromEmail, _settings.Password),
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
        }
    }
}