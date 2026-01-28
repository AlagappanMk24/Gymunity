using Gymunity.Domain.Entities.Trainer;

namespace Gymunity.Domain.Interfaces
{
    public interface IReviewAdminRepository : IRepository<TrainerReview>
    {
        Task<IReadOnlyList<TrainerReview>> GetAllPendingAsync();
    }
}