using Gymunity.Application.DTOs.ClientDto;

namespace Gymunity.Application.Contracts.Services.Client
{
    public interface IOnboardingService
    {
        Task<bool> IsProfileOnboardingCompletedAsync(string userId);
        Task<bool> CompleteOnboardingAsync(string userId, OnboardingRequest request);
    }
}
