using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Interfaces.Client;
using Gymunity.Infrastructure.Data.Context;

namespace Gymunity.Infrastructure.Repositories.Client
{
    public class WorkoutLogRepository(AppDbContext dbcontext)
        : Repository<WorkoutLog>(dbcontext), IWorkoutLogRepository
    {
        private readonly AppDbContext _dbcontext = dbcontext;

        async Task<WorkoutLog?> IWorkoutLogRepository.GetByIdAsync(long id) 
            => await _dbcontext.FindAsync<WorkoutLog>(id);
    }
}