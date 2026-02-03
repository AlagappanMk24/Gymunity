using AutoMapper;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Trainers;
using Gymunity.Domain;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces.Trainer;

namespace Gymunity.Application.Services.Trainer
{
    public class ReviewTrainerService(IReviewTrainerRepository repo, IUnitOfWork uow, IMapper mapper) : IReviewTrainerService
    {
        private readonly IReviewTrainerRepository _repo = repo;
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        public async Task<TrainerReviewResponse> CreateAsync(string clientUserId, int trainerId, TrainerReviewCreateRequest request)
        {
            // keep available for client-created path; trainers cannot call this endpoint
            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between1 and5");

            var review = new TrainerReview
            {
                TrainerId = trainerId,
                ClientId = int.Parse(clientUserId),
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _repo.Add(review);
            await _uow.CompleteAsync();
            return _mapper.Map<TrainerReviewResponse>(review);
        }

        public async Task<IEnumerable<TrainerAreaReviewResponse>> GetApprovedForTrainerAsync(int trainerId)
        {
            var list = await _repo.GetByTrainerIdAsync(trainerId);
            var approved = list.Where(r => r.IsApproved);
            return approved.Select(r => _mapper.Map<TrainerAreaReviewResponse>(r));
        }
    }
}