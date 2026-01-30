using Gymunity.Domain.Entities.Client;

namespace Gymunity.Domain.Interfaces.Client
{
    public interface IWorkoutLogRepository : IRepository<WorkoutLog>
    {
        Task<WorkoutLog?> GetByIdAsync(long id);
    }
}