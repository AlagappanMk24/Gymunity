using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces.Trainer;
using Gymunity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Repositories.Trainer
{
    public class ReviewTrainerRepository(AppDbContext dbContext) : Repository<TrainerReview>(dbContext), IReviewTrainerRepository
    {
        public async Task<IReadOnlyList<TrainerReview>> GetByTrainerIdAsync(int trainerId)
        {
            return await _context.Set<TrainerReview>()
            .Where(r => r.TrainerId == trainerId)
            .Include(r => r.Client)
            .ToListAsync();
        }
    }
}