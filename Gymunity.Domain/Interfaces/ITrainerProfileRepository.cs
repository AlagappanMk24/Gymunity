using Gymunity.Domain.Entities.Trainer;

namespace Gymunity.Domain.Interfaces
{
    public interface ITrainerProfileRepository : IRepository<TrainerProfile>
    {
        Task<TrainerProfile?> GetByHandleAsync(string handle);
        Task<bool> HandleExistsAsync(string handle);
        Task<IReadOnlyList<TrainerProfile>> GetTopRatedTrainersAsync(int count);
    }
}