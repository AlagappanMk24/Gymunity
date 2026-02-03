using Gymunity.Domain.Entities.Trainer;

namespace Gymunity.Domain.Interfaces.Admin
{
    public interface IReviewAdminRepository : IRepository<TrainerReview>
    {
        Task<IReadOnlyList<TrainerReview>> GetAllPendingAsync();
    }
}