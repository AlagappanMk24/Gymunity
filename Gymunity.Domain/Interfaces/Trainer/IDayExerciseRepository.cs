using Gymunity.Domain.Entities.ProgramAggregate;

namespace Gymunity.Domain.Interfaces.Trainer
{
    public interface IDayExerciseRepository : IRepository<ProgramDayExercise>
    {
        Task<IReadOnlyList<ProgramDayExercise>> GetByDayIdAsync(int dayId);
    }
}