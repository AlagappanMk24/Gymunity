using Gymunity.Domain.Entities.Trainer;

namespace Gymunity.Domain.Interfaces.Client.Admin
{
    public interface IReviewAdminRepository : IRepository<TrainerReview>
    {
        Task<IReadOnlyList<TrainerReview>> GetAllPendingAsync();
    }
}