using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Interfaces.Trainer;
using Gymunity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Repositories.Trainer
{
    public class ExerciseLibraryRepository : Repository<Exercise>, IExerciseLibraryRepository
    {
        public ExerciseLibraryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Exercise>> SearchByNameAsync(string? name, string? trainerId = null)
        {
            var query = _context.Set<Exercise>().AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(e => e.Name.Contains(name));
            if (!string.IsNullOrWhiteSpace(trainerId))
                query = query.Where(e => e.TrainerId == trainerId);
            return await query.ToListAsync();
        }
    }
}