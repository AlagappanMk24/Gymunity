using Gymunity.Application.DTOs.Email;

namespace Gymunity.Application.Contracts.ExternalServices.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailRequest request);
        Task SendBulkEmailAsync(List<EmailRequest> requests);
    }
}