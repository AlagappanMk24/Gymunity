using AutoMapper;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.Trainers;
using Gymunity.Domain;
using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces.Client;

namespace Gymunity.Application.Services.Client
{
    public class ReviewClientService(IReviewClientRepository clientRepository, IUnitOfWork uow, IMapper mapper) : IReviewClientService
    {
        private readonly IReviewClientRepository _clientRepository = clientRepository;
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        public async Task<TrainerReviewResponse> CreateAsync(string clientUserId, int trainerId, TrainerReviewCreateRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between1 and5");

            // clientUserId is AppUser.Id (string). Resolve or create ClientProfile for this user.
            var clientRepo = _uow.Repository<ClientProfile>();
            var allClients = await clientRepo.GetAllAsync();
            var clientProfile = allClients.FirstOrDefault(c => c.UserId == clientUserId);
            if (clientProfile == null)
            {
                clientProfile = new ClientProfile
                {
                    UserId = clientUserId,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                clientRepo.Add(clientProfile);
                await _uow.CompleteAsync();
            }

            // check duplicate: a client can only have one review per trainer (not deleted)
            var existing = (await _clientRepository.GetByTrainerIdAsync(trainerId))
            .FirstOrDefault(r => r.ClientId == clientProfile.Id && !r.IsDeleted);
            if (existing != null)
            {
                throw new ArgumentException("You have already submitted a review for this trainer.");
            }

            var review = new TrainerReview
            {
                TrainerId = trainerId,
                ClientId = clientProfile.Id,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTimeOffset.UtcNow,
                IsApproved = false // new reviews require admin approval
            };

            _clientRepository.Add(review);
            await _uow.CompleteAsync();
            return _mapper.Map<TrainerReviewResponse>(review);
        }

        public async Task<TrainerReviewResponse?> UpdateAsync(string clientUserId, int reviewId, TrainerReviewCreateRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between1 and5");

            var review = await _uow.Repository<TrainerReview>().GetByIdAsync(reviewId);
            if (review == null || review.IsDeleted) return null;

            // ensure the review belongs to the authenticated client's profile
            var clientRepo = _uow.Repository<ClientProfile>();
            var allClients = await clientRepo.GetAllAsync();
            var clientProfile = allClients.FirstOrDefault(c => c.UserId == clientUserId);
            if (clientProfile == null) return null;
            if (review.ClientId != clientProfile.Id) return null;

            // update fields and require admin re-approval
            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.IsEdited = true;
            review.EditedAt = DateTimeOffset.UtcNow;
            review.IsApproved = false; // require admin approval after client edits
            review.ApprovedAt = null;
            _uow.Repository<TrainerReview>().Update(review);
            await _uow.CompleteAsync();
            return _mapper.Map<TrainerReviewResponse>(review);
        }

        public async Task<bool> DeleteAsync(string clientUserId, int reviewId)
        {
            var review = await _uow.Repository<TrainerReview>().GetByIdAsync(reviewId);
            if (review == null || review.IsDeleted) return false;

            var clientRepo = _uow.Repository<ClientProfile>();
            var allClients = await clientRepo.GetAllAsync();
            var clientProfile = allClients.FirstOrDefault(c => c.UserId == clientUserId);
            if (clientProfile == null) return false;
            if (review.ClientId != clientProfile.Id) return false;

            // soft delete
            review.IsDeleted = true;
            _uow.Repository<TrainerReview>().Update(review);
            await _uow.CompleteAsync();
            return true;
        }
    }
}
