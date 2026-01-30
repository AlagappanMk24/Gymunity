using Gymunity.Application.DTOs.Trainers;

namespace Gymunity.Application.Contracts.Services.Client
{
    public interface IReviewClientService
    {
        Task<TrainerReviewResponse> CreateAsync(string clientUserId, int trainerId, TrainerReviewCreateRequest request);
        Task<TrainerReviewResponse?> UpdateAsync(string clientUserId, int reviewId, TrainerReviewCreateRequest request);
        Task<bool> DeleteAsync(string clientUserId, int reviewId);
    }
}