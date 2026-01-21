using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.Contracts.Services;
using Gymunity.Application.DTOs.Email;
using Microsoft.Extensions.Logging;

namespace Gymunity.Application.Services
{
    public class EmailTemplateService(IEmailService emailService, ILogger<EmailTemplateService> logger) : IEmailTemplateService
    {
        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<EmailTemplateService> _logger = logger;

        /// Send subscription confirmation email to client
        /// Returns true if successful
        public async Task<bool> SendSubscriptionConfirmationAsync(
            SubscriptionConfirmationEmail data)
        {
            try
            {
                var htmlBody = BuildClientConfirmationEmail(data);

                var emailRequest = new EmailRequest
                {
                    ToEmail = data.ClientEmail,
                    ToName = data.ClientName,
                    Subject = $"🎉 Subscription Confirmed - {data.PackageName}",
                    Body = htmlBody,
                    IsHtml = true
                };

                // IEmailService.SendEmailAsync returns Task. Await it and treat completion as success.
                await _emailService.SendEmailAsync(emailRequest);

                _logger.LogInformation(
                    "✅ Subscription confirmation email sent to {Email}",
                    data.ClientEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error sending subscription confirmation to {Email}",
                    data.ClientEmail);
                return false;
            }
        }

        /// Send trainer notification email
        /// Returns true if successful
        public async Task<bool> SendTrainerNotificationAsync(
            TrainerNotificationEmail data)
        {
            try
            {
                var htmlBody = BuildTrainerNotificationEmail(data);

                var emailRequest = new EmailRequest
                {
                    ToEmail = data.TrainerEmail,
                    ToName = data.TrainerName,
                    Subject = $"💰 New Subscriber - {data.ClientName}",
                    Body = htmlBody,
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(emailRequest);

                _logger.LogInformation(
                    "✅ Trainer notification sent to {Email}",
                    data.TrainerEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error sending trainer notification to {Email}",
                    data.TrainerEmail);
                return false;
            }
        }

        /// Send payment failure email
        /// Returns true if successful
        public async Task<bool> SendPaymentFailureEmailAsync(
            string clientEmail,
            string clientName,
            string packageName,
            string failureReason)
        {
            try
            {
                var htmlBody = BuildPaymentFailureEmail(clientName, packageName, failureReason);

                var emailRequest = new EmailRequest
                {
                    ToEmail = clientEmail,
                    ToName = clientName,
                    Subject = "❌ Payment Failed - Action Required",
                    Body = htmlBody,
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(emailRequest);

                _logger.LogInformation(
                    "✅ Payment failure email sent to {Email}",
                    clientEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error sending payment failure email to {Email}",
                    clientEmail);
                return false;
            }
        }

        #region HTML Email Templates
        private string BuildClientConfirmationEmail(SubscriptionConfirmationEmail data)
        {
            return $@"<!DOCTYPE html><html><head><meta charset='utf-8'><style>body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}.container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}.header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}.content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}.card {{ background: white; padding: 20px; margin: 20px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}.amount {{ font-size: 24px; font-weight: bold; color: #667eea; }}.button {{ display: inline-block; background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}</style></head><body><div class='container'><div class='header'><h1>🎉 Subscription Confirmed!</h1><p>Welcome to {data.TrainerName}'s Program</p></div><div class='content'><p>Hi <strong>{data.ClientName}</strong>,</p><p>Your subscription is now active!</p><div class='card'><p><strong>Package:</strong> {data.PackageName}</p><p><strong>Trainer:</strong> {data.TrainerName}</p><p><strong>Amount:</strong> <span class='amount'>{data.Amount:N2} {data.Currency}</span></p><p><strong>Valid Until:</strong> {data.SubscriptionEndDate:dd MMM yyyy}</p></div><div style='text-align: center;'><a href='https://yourdomain.com/my-programs' class='button'>Start Training</a></div></div></div></body></html>";
        }
        private string BuildTrainerNotificationEmail(TrainerNotificationEmail data)
        {
            return $@"<!DOCTYPE html><html><head><meta charset='utf-8'><style>body {{ font-family: Arial, sans-serif; color: #333; }}.container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}.header {{ background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}.content {{ background: #f9f9f9; padding: 30px; }}.card {{ background: white; padding: 20px; margin: 20px 0; border-radius: 8px; }}.highlight {{ font-size: 28px; font-weight: bold; color: #11998e; text-align: center; padding: 20px; }}</style></head><body><div class='container'><div class='header'><h1>💰 New Subscriber!</h1></div><div class='content'><p>Hi <strong>{data.TrainerName}</strong>,</p><p><strong>{data.ClientName}</strong> subscribed to <strong>{data.PackageName}</strong>!</p><div class='card'><p><strong>Total Amount:</strong> {data.Amount:N2} {data.Currency}</p></div><div class='highlight'>Your Payout: {data.TrainerPayout:N2} {data.Currency}</div></div></div></body></html>";
        }
        private string BuildPaymentFailureEmail(
            string clientName,
            string packageName,
            string failureReason)
        {
            return $@"<!DOCTYPE html><html><head><meta charset='utf-8'><style>body {{ font-family: Arial, sans-serif; color: #333; }}.container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}.header {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 30px; text-align: center; }}.content {{ padding: 30px; }}.alert {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}.button {{ display: inline-block; background: #f5576c; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; }}</style></head><body><div class='container'><div class='header'><h1>❌ Payment Failed</h1></div><div class='content'><p>Hi <strong>{clientName}</strong>,</p><p>Payment for <strong>{packageName}</strong> failed.</p><div class='alert'><strong>Reason:</strong> {failureReason}</div><div style='text-align: center;'><a href='https://yourdomain.com/retry-payment' class='button'>Retry Payment</a></div></div></div></body></html>";
        }

        #endregion
    }
}