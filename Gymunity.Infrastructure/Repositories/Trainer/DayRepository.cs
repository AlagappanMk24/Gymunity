using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Interfaces.Trainer;
using Gymunity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Repositories.Trainer
{
    public class DayRepository(AppDbContext context) : Repository<ProgramDay>(context), IDayRepository
    {
        public async Task<IReadOnlyList<ProgramDay>> GetByWeekIdAsync(int weekId)
        {
            return await _context.ProgramDays.Where(d => d.ProgramWeekId == weekId).ToListAsync();
        }

        public async Task<ProgramDay?> GetWithExercisesAsync(int id)
        {
            return await _context.ProgramDays.Include(d => d.Exercises).FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}