using Gymunity.Domain.Entities.Trainer;

namespace Gymunity.Domain.Interfaces.Client
{
    public interface IReviewClientRepository : IRepository<TrainerReview>
    {
        Task<IReadOnlyList<TrainerReview>> GetByTrainerIdAsync(int trainerId);
    }
}