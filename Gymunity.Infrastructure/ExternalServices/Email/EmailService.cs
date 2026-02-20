using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.DTOs.Email;
using Gymunity.Infrastructure.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Gymunity.Infrastructure.ExternalServices.Email
{
    public class EmailService(IOptions<EmailSettings> emailOptions, IEmailTemplateRenderer templateService, ILogger<EmailService> logger) : IEmailService
    {
        private readonly EmailSettings _settings = emailOptions.Value;
        private readonly IEmailTemplateRenderer _templateService = templateService;
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
        public async Task SendTemplatedEmailAsync(string toEmail, string toName, string subject,
           string template, Dictionary<string, string> placeholders = null)
        {
            // Apply placeholders if provided
            if (placeholders != null)
            {
                foreach (var placeholder in placeholders)
                {
                    template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
                }
            }

            var request = new EmailRequest
            {
                ToEmail = toEmail,
                ToName = toName,
                Subject = subject,
                Body = template,
                IsHtml = true
            };

            await SendEmailAsync(request);
        }
        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.FromEmail, _settings.Password),
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
        }
    }
}