using Gymunity.Application.DTOs.Trainers;

namespace Gymunity.Application.Contracts.Services.Trainer
{
    public interface ITrainerProfileService
    {
        Task<IEnumerable<TrainerReviewResponse>?> GetAllReviews(int trainerProfileId);
        Task<TrainerFullProfileResponse?> GetFullProfileByUserIdAsync(string userId);
        Task<TrainerProfileDetailResponse?> GetProfileByUserId(string userId);
        Task<TrainerProfileDetailResponse?> GetProfileById(int id);
        Task<TrainerProfileDetailResponse> CreateProfile(CreateTrainerProfileRequest request);
        Task<TrainerProfileDetailResponse> UpdateProfile(int profileId, UpdateTrainerProfileRequest request);
        Task<bool> DeleteProfile(int profileId);
        Task<IReadOnlyList<SubscriberResponse>> GetSubscribersByTrainerIdAsync(string trainerId);
        Task<TrainerProfileDetailResponse> UpdateStatus(int profileId, UpdateStatusRequest request);
        Task<bool> DeleteStatus(int profileId);
    }
}
