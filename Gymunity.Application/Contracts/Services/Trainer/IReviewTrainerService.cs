using Gymunity.Application.DTOs.Trainers;

namespace Gymunity.Application.Contracts.Services.Trainer
{
    public interface IReviewTrainerService
    {
        Task<TrainerReviewResponse> CreateAsync(string clientUserId, int trainerId, TrainerReviewCreateRequest request);
        Task<IEnumerable<TrainerAreaReviewResponse>> GetApprovedForTrainerAsync(int trainerId);
    }
}