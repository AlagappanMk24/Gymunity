using AutoMapper;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.Trainers;
using Gymunity.Application.Specifications.Subscriptions;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;

namespace Gymunity.Application.Services.Client
{
    public class ClientTrainersService(IUnitOfWork unitOfWork, IMapper mapper) : IClientTrainersService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<TrainerBriefResponse>> GetClientTrainers(string userId)
        {
            // Get all active subscriptions for the client
            var spec = new ClientSubscriptionsSpecs(
                userId, 
               SubscriptionStatus.Active);
            
            var subscriptions = await _unitOfWork.Repository<Subscription>().GetAllWithSpecsAsync(spec);

            // Extract unique trainers from subscriptions
            var trainerProfiles = subscriptions
                .Where(s => s.Package?.Trainer != null)
                .Select(s => s.Package!.Trainer)
                .DistinctBy(t => t.Id)
                .ToList();

            // Map to TrainerBriefResponse using AutoMapper
            var result = _mapper.Map<List<TrainerBriefResponse>>(trainerProfiles);

            return result;
        }
    }
}