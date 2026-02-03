using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces;
using Gymunity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Repositories
{
    public class GuestReviewRepository(AppDbContext dbContext) : Repository<TrainerReview>(dbContext), IGuestReviewRepository
    {
        public async Task<IReadOnlyList<TrainerReview>> GetApprovedByTrainerIdAsync(int trainerId)
        {
            return await _context.Set<TrainerReview>()
            .Where(r => r.TrainerId == trainerId && r.IsApproved)
            .Include(r => r.Client).ThenInclude(c => c.User)
            .ToListAsync();
        }

        public async Task<IReadOnlyList<TrainerProfile>> GetTopTrainersByClientsAsync(int top)
        {
            return await _context.Set<TrainerProfile>()
            .OrderByDescending(tp => tp.TotalClients)
            .Take(top)
            .ToListAsync();
        }
    }
}