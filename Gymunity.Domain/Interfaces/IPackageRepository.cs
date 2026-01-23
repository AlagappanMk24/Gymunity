using Gymunity.Domain.Entities.Trainer;

namespace Gymunity.Domain.Interfaces
{
    public interface IPackageRepository : IRepository<Package>
    {
        Task<IReadOnlyList<Package>> GetByTrainerIdAsync(int trainerId);
        Task<IReadOnlyList<Package>> GetAllActiveWithProgramsAsync();
        Task<Package?> GetByIdWithProgramsAsync(int id);
    }
}