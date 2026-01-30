using AutoMapper;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.ClientDto;
using Gymunity.Application.Specifications.Client;
using Gymunity.Domain;
using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Interfaces.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Gymunity.Application.Services.Client
{
    public class ClientProfileService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ClientProfileService> logger, UserManager<AppUser> userManager) : IClientProfileService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<ClientProfileService> _logger = logger;
        private readonly UserManager<AppUser> _userManager = userManager;
        public async Task<ClientProfileResponse?> GetClientProfileAsync(string userId)
        {
            var clientSpec = new ClientWithUserSpecs(c => c.UserId == userId);
            
            var clientProfile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>().
                GetWithSpecsAsync(clientSpec);

            if(clientProfile == null)
                return null;

            return _mapper.Map<ClientProfileResponse>(clientProfile);
        }
        public async Task<ClientProfileDashboardResponse> GetDashboardAsync(string userId)
        {
            // Get profile with User and BodyStatLogs included
            var specs = new ClientWithUserSpecs(c => c.UserId == userId);
            var profile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>()
                .GetWithSpecsAsync(specs);

            if (profile == null)
            {
                _logger.LogWarning("Client profile not found for UserId: {UserId}", userId);
                throw new InvalidOperationException("Client profile not found");
            }

            // Map to dashboard response using AutoMapper
            var dashboard = _mapper.Map<ClientProfileDashboardResponse>(profile);

            _logger.LogInformation("Dashboard retrieved successfully for ClientId: {ClientId}", profile.Id);

            return dashboard;
        }
        public async Task<ClientProfileResponse?> CreateClientProfileAsync(string userId, ClientProfileRequest request)
        {
            var spec = new ClientWithUserSpecs(c => c.UserId == userId);

            var existingProfile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>()
                .GetWithSpecsAsync(spec);

            if (existingProfile != null)
                return _mapper.Map<ClientProfileResponse>(existingProfile);


            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null)
                return null;

            var clientProfile = _mapper.Map<ClientProfile>(request);
            clientProfile.UserId = userId;
            clientProfile.CreatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ClientProfile, IClientProfileRepository>().Add(clientProfile);

            try
            {
            await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex ,"Error occured while Saving Changes");
                return null;
            }

            return _mapper.Map<ClientProfileResponse>(clientProfile);
        }
        public async Task<ClientProfileResponse?> UpdateClientProfileAsync(string userId, ClientProfileRequest request)
        {
            var spec = new ClientWithUserSpecs(c => c.UserId == userId);

            var clientProfile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>()
                .GetWithSpecsAsync(spec);

            if (clientProfile is null)
                return null;

            _mapper.Map(request,clientProfile);
          
            _unitOfWork.Repository<ClientProfile>().Update(clientProfile);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex ,"Error occured while Saving Changes");
                return null;
            }

            return _mapper.Map<ClientProfileResponse>(clientProfile);
        }

        //public async Task<bool> UpdateClientInfoAsync(string userId, string photoUrl)
        //{

        //    _userManager.FindByIdAsync(userId);

        //    await _unitOfWork.CompleteAsync();
        //    return true;
        //}

        //public async Task<bool> UpdateClientGoalAsync(string userId, string goal)
        //{
        //    var spec = new ClientWithUserSpecs(c => c.UserId == userId);

        //    var clientProfile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>().
        //        GetWithSpecsAsync(spec);

        //    if (clientProfile is null)
        //        return false;

            

        //    await _unitOfWork.CompleteAsync();
        //    return true;
        //}
        //public async Task<bool> UpdateExperienceLevelAsync(string userId, string level)
        //{
        //    var spec = new ClientWithUserSpecs(c => c.UserId == userId);

        //    var clientProfile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>().
        //        GetWithSpecsAsync(spec);

        //    if (clientProfile is null)
        //        return false;

        //    clientProfile.ExperienceLevel = level;

        //    await _unitOfWork.CompleteAsync();
        //    return true;
        //}
        //public async Task<bool> UpdateAnthropometricsAsync(string userId, int? height, decimal? weight)
        //{
        //    var spec = new ClientWithUserSpecs(c => c.UserId == userId);

        //    var clientProfile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>().
        //        GetWithSpecsAsync(spec);

        //    if (clientProfile is null)
        //        return false;

        //    clientProfile.HeightCm = height;
        //    clientProfile.StartingWeightKg = weight;

        //    await _unitOfWork.CompleteAsync();
        //    return true;
        //}
        public async Task<bool> DeleteProfileAsync(string userId)
        {
            var profileSpecs = new ClientWithUserSpecs(c => c.UserId == userId);

            var profile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>()
                .GetWithSpecsAsync(profileSpecs);

                if (profile == null)
                    throw new InvalidOperationException("Client profile not found");

            _unitOfWork.Repository<ClientProfile>().Delete(profile);
            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex ,"Error occured while Saving Changes");
                return false;
            }
            return true;

        }
    }
}
