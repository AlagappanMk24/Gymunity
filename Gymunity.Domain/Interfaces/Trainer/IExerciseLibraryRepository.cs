using Gymunity.Domain.Entities.ProgramAggregate;

namespace Gymunity.Domain.Interfaces.Trainer
{
    public interface IExerciseLibraryRepository : IRepository<Exercise>
    {
        Task<IReadOnlyList<Exercise>> SearchByNameAsync(string? name, string? trainerId = null);
    }
}