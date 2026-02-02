using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Interfaces.Trainer;
using Gymunity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Repositories.Trainer
{
    public class DayExerciseRepository(AppDbContext context) : Repository<ProgramDayExercise>(context), IDayExerciseRepository
    {
        public async Task<IReadOnlyList<ProgramDayExercise>> GetByDayIdAsync(int dayId)
        {
            return await _context.ProgramDayExercises.Where(e => e.ProgramDayId == dayId).ToListAsync();
        }
    }
}