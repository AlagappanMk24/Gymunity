using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.ClientDto;
using Gymunity.Application.Specifications.Client;
using Gymunity.Domain;
using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Interfaces.Client;
using Microsoft.Extensions.Logging;

namespace Gymunity.Application.Services.Client
{
    public class OnboardingService(IUnitOfWork unitOfWork, ILogger<OnboardingService> logger) : IOnboardingService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger _logger = logger;

        public async Task<bool> IsProfileOnboardingCompletedAsync(string userId)
        {
            var spec = new ClientWithUserSpecs(cp => cp.UserId == userId);

            var clientProfile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>()
                                                  .GetWithSpecsAsync(spec);

            if (clientProfile == null)
                return false;

            bool isCompleted =
                clientProfile.HeightCm.HasValue &&
                clientProfile.StartingWeightKg.HasValue &&
                !string.IsNullOrEmpty(clientProfile.Gender) &&
                !string.IsNullOrEmpty(clientProfile.Goal) &&
                !string.IsNullOrEmpty(clientProfile.ExperienceLevel);

            clientProfile.IsOnboardingCompleted = true;
            return isCompleted;
        }

        public async Task<bool> CompleteOnboardingAsync(string userId, OnboardingRequest request)
        {
            var clientSpec = new ClientWithUserSpecs(c => c.UserId == userId);

            var profile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>()
                .GetWithSpecsAsync(clientSpec);

            if (profile == null)
                throw new Exception("Client profile not found");

            if (profile.IsOnboardingCompleted)
                return false;

            profile.HeightCm = request.HeightCm;
            profile.StartingWeightKg = request.StartingWeightKg;
            profile.Goal = request.Goal;
            profile.ExperienceLevel = request.ExperienceLevel;
            profile.IsOnboardingCompleted = true;

            _unitOfWork.Repository<ClientProfile>().Update(profile);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while Saving Changes");
                return false;
            }
            return true;
        }
    }
}