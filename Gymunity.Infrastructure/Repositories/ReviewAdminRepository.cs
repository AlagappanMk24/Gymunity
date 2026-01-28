using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces;
using Gymunity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Repositories
{
    public class ReviewAdminRepository(AppDbContext dbContext) : Repository<TrainerReview>(dbContext), IReviewAdminRepository
    {
        public async Task<IReadOnlyList<TrainerReview>> GetAllPendingAsync()
        {
            return await _context.Set<TrainerReview>()
            .Where(r => !r.IsApproved)
            .Include(r => r.Client)
            .Include(r => r.Trainer)
            .ToListAsync();
        }
    }
}