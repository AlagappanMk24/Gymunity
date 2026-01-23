using Gymunity.Application.DTOs.Trainers;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain.Entities.Trainer;

namespace Gymunity.Application.Contracts.Services.Admin
{
    public interface ITrainerAdminService
    {
        Task<IEnumerable<TrainerProfileDetailResponse>> GetAllTrainersAsync(TrainerFilterSpecs specs);
        Task<TrainerProfileDetailResponse?> GetTrainerByIdAsync(int id);

        Task<(TrainerProfileDetailResponse? trainer, List<TrainerReview> reviews, decimal totalEarnings, 
            decimal platformFees, int completedPaymentsCount)> GetTrainerDetailsForAdminAsync(int id);
        Task<int> GetTrainerCountAsync(TrainerFilterSpecs specs);
        Task<bool> VerifyTrainerAsync(int trainerId);
        Task<bool> RejectTrainerAsync(int trainerId);
        Task<bool> SuspendTrainerAsync(int trainerId, bool suspend = true);
        Task<IEnumerable<TrainerProfileDetailResponse>> GetPendingTrainersAsync(int pageNumber = 1, int pageSize = 10);
        Task<int> GetPendingTrainerCountAsync();
        Task<IEnumerable<TrainerProfileDetailResponse>> GetVerifiedTrainersAsync(int pageNumber = 1,int pageSize = 10);
        Task<IEnumerable<TrainerProfileDetailResponse>> GetSuspendedTrainersAsync(int pageNumber = 1, int pageSize = 10);
        Task<int> GetSuspendedTrainerCountAsync();
        Task<IEnumerable<TrainerProfileDetailResponse>> SearchTrainersAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
    }
}