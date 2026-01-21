using Gymunity.Application.DTOs.Email;

namespace Gymunity.Application.Contracts.Services
{
    public interface IEmailTemplateService
    {
        Task<bool> SendSubscriptionConfirmationAsync(SubscriptionConfirmationEmail data);
        Task<bool> SendTrainerNotificationAsync(TrainerNotificationEmail data);
        Task<bool> SendPaymentFailureEmailAsync(string clientEmail, string clientName, string packageName, string failureReason);
    }
}