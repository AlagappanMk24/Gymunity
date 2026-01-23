using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces;
using Gymunity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Repositories
{
    public class PackageRepository(AppDbContext dbContext) : Repository<Package>(dbContext), IPackageRepository
    {
        public async Task<IReadOnlyList<Package>> GetByTrainerIdAsync(int trainerId)
        {
            return await _Context.Packages
            .Where(p => p.TrainerId == trainerId && !p.IsDeleted)
            .Include(p => p.PackagePrograms)
            .ThenInclude(pp => pp.Program)
            .Include(p => p.Trainer)
            .ThenInclude(t => t.User)
            .ToListAsync();
        }

        public async Task<IReadOnlyList<Package>> GetAllActiveWithProgramsAsync()
        {
            return await _Context.Packages
            .Where(p => p.IsActive && !p.IsDeleted)
            .Include(p => p.PackagePrograms)
            .ThenInclude(pp => pp.Program)
            .Include(p => p.Trainer)
            .ThenInclude(t => t.User)
            .ToListAsync();
        }

        public async Task<Package?> GetByIdWithProgramsAsync(int id)
        {
            return await _Context.Packages
            .Where(p => p.Id == id && !p.IsDeleted)
            .Include(p => p.PackagePrograms)
            .ThenInclude(pp => pp.Program)
            .Include(p => p.Trainer)
            .ThenInclude(t => t.User)
            .FirstOrDefaultAsync();
        }
    }
}
