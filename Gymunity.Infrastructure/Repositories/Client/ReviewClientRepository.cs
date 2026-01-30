using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces.Client;
using Gymunity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Repositories.Client
{
    public class ReviewClientRepository(AppDbContext dbContext) : Repository<TrainerReview>(dbContext), IReviewClientRepository
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
