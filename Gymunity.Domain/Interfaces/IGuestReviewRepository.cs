using Gymunity.Domain.Entities.Trainer;

namespace Gymunity.Domain.Interfaces
{
    public interface IGuestReviewRepository : IRepository<TrainerReview>
    {
        Task<IReadOnlyList<TrainerReview>> GetApprovedByTrainerIdAsync(int trainerId);
        Task<IReadOnlyList<TrainerProfile>> GetTopTrainersByClientsAsync(int top);
    }
}