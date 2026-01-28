using Gymunity.Application.DTOs.Admin;
using Gymunity.Application.DTOs.Trainers;

namespace Gymunity.Application.Contracts.Services.Admin
{
    public interface IReviewAdminService
    {
        Task<AdminReviewActionResponse?> ApproveAsync(int reviewId);
        Task<AdminReviewActionResponse?> RejectAsync(int reviewId);
        Task<bool> DeletePermanentAsync(int reviewId);
        Task<IReadOnlyList<AdminReviewActionResponse>> GetPendingAsync();
        Task<IReadOnlyList<TrainerReviewResponse>> GetAllPendingAsync();
    }
}