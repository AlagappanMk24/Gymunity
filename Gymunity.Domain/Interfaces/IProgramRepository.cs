using Gymunity.Domain.Entities.ProgramAggregate;

namespace Gymunity.Domain.Interfaces
{
     public interface IProgramRepository : IRepository<Program>
     {
        Task<int> GetWeeksCount(int programId);
        Task<int> GetTotalExercisesCount(int programId);
        Task<IReadOnlyList<Program>> GetByTrainerAsync(string trainerUserId);
        Task<IReadOnlyList<Program>> GetByTrainerAsyncProfileId(int trainerProfileId);
        Task<Program?> GetByIdWithIncludesAsync(int id);
        Task<IReadOnlyList<Program>> SearchAsync(string? term);
        Task<bool> ExistsByTitleAsync(string title);
     }
}