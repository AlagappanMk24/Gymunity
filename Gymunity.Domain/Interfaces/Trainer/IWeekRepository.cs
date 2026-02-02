using Gymunity.Domain.Entities.ProgramAggregate;

namespace Gymunity.Domain.Interfaces.Trainer
{
    public interface IWeekRepository : IRepository<ProgramWeek>
    {
        Task<IReadOnlyList<ProgramWeek>> GetByProgramIdAsync(int programId);
        Task<ProgramWeek?> GetWithDaysAsync(int id);
    }
}