using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces.Trainer;
using Gymunity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Repositories.Trainer
{
    public class TrainerProfileRepository(AppDbContext dbContext)
        : Repository<TrainerProfile>(dbContext), ITrainerProfileRepository
    {
        public async Task<TrainerProfile?> GetByHandleAsync(string handle)
        {
            return await _context.TrainerProfiles
                .FirstOrDefaultAsync(tp => tp.Handle == handle);
        }

        public async Task<IReadOnlyList<TrainerProfile>> GetTopRatedTrainersAsync(int count)
        {
            return await _context.TrainerProfiles
                .OrderByDescending(tp => tp.RatingAverage)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> HandleExistsAsync(string handle)
        {
            return await _context.TrainerProfiles
                .AnyAsync(tp => tp.Handle == handle);
        }
    }
}