using Gymunity.Domain.Entities.ProgramAggregate;

namespace Gymunity.Domain.Interfaces.Trainer
{
    public interface IDayRepository : IRepository<ProgramDay>
    {
        Task<IReadOnlyList<ProgramDay>> GetByWeekIdAsync(int weekId);
        Task<ProgramDay?> GetWithExercisesAsync(int id);
    }
}