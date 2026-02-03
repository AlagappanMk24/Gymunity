using Gymunity.Application.DTOs.Guest;

namespace Gymunity.Application.Contracts.Services
{
    public interface IGuestReviewService
    {
        Task<GuestReviewsByTrainerResponse> GetApprovedReviewsByTrainerAsync(int trainerProfileId);
        Task<TopTrainersResponse> GetTopTrainersAsync();
    }
}