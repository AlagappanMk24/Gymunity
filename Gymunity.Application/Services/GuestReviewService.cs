using AutoMapper;
using Gymunity.Application.Contracts.Services;
using Gymunity.Application.DTOs.Guest;
using Gymunity.Domain;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces;

namespace Gymunity.Application.Services
{
    public class GuestReviewService(IGuestReviewRepository repo, IUnitOfWork uow, IMapper mapper) : IGuestReviewService
    {
        private readonly IGuestReviewRepository _repo = repo;
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        public async Task<GuestReviewsByTrainerResponse> GetApprovedReviewsByTrainerAsync(int trainerProfileId)
        {
            // Use unit of work to obtain repo (per requirement)
            var repo = _uow.Repository<TrainerReview, IGuestReviewRepository>();
            var reviews = await repo.GetApprovedByTrainerIdAsync(trainerProfileId);
            var items = reviews.Select(r => _mapper.Map<GuestReviewResponseItem>(r)).ToArray();
            return new GuestReviewsByTrainerResponse { TrainerProfileId = trainerProfileId, Reviews = items };
        }

        public async Task<TopTrainersResponse> GetTopTrainersAsync()
        {
            var topList = await _repo.GetTopTrainersByClientsAsync(10);
            var dtos = topList.Select(tp => _mapper.Map<TopTrainerResponse>(tp)).ToArray();
            return new TopTrainersResponse { Trainers = dtos };
        }
    }
}