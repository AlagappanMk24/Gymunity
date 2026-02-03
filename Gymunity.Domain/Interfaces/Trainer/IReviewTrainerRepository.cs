using Gymunity.Domain.Entities.Trainer;

namespace Gymunity.Domain.Interfaces.Trainer
{
    public interface IReviewTrainerRepository : IRepository<TrainerReview>
    {
        Task<IReadOnlyList<TrainerReview>> GetByTrainerIdAsync(int trainerId);
    }
}