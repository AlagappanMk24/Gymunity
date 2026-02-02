using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Interfaces.Trainer;
using Gymunity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Repositories.Trainer
{
    public class WeekRepository(AppDbContext context) : Repository<ProgramWeek>(context), IWeekRepository
    {
        public async Task<IReadOnlyList<ProgramWeek>> GetByProgramIdAsync(int programId)
        {
            return await _context.ProgramWeeks.Where(w => w.ProgramId == programId).ToListAsync();
        }

        public async Task<ProgramWeek?> GetWithDaysAsync(int id)
        {
            return await _context.ProgramWeeks.Include(w => w.Days).FirstOrDefaultAsync(w => w.Id == id);
        }
    }
}