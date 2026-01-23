using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Interfaces;
using Gymunity.Infrastructure.Data.Context;
using Gymunity.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ITI.Gymunity.FP.Infrastructure.Repositories
{
	public class ProgramRepository(AppDbContext context) : Repository<Program>(context), IProgramRepository
	{

		// legacy: still support trainerId as user id string by joining via TrainerProfile
		public async Task<IReadOnlyList<Program>> GetByTrainerAsync(string trainerUserId)
		{
			return await _context.Programs.Where(p => p.TrainerProfileId != null && p.TrainerProfile.UserId == trainerUserId).ToListAsync();
		}

		public async Task<IReadOnlyList<Program>> GetByTrainerAsyncProfileId(int trainerProfileId)
		{
			return await _context.Programs.Where(p => p.TrainerProfileId == trainerProfileId).ToListAsync();
		}

		public async Task<Program?> GetByIdWithIncludesAsync(int id)
		{
			return await _context.Programs.Include(p => p.Weeks).ThenInclude(w => w.Days).ThenInclude(d => d.Exercises)
			.FirstOrDefaultAsync(p => p.Id == id);
		}

		public async Task<IReadOnlyList<Program>> SearchAsync(string? term)
		{
			var query = _context.Programs.AsQueryable();
			if (!string.IsNullOrWhiteSpace(term))
			query = query.Where(p => p.Title.Contains(term) || p.Description.Contains(term));
			return await query.ToListAsync();
		}

		public async Task<bool> ExistsByTitleAsync(string title)
		{
			if (string.IsNullOrWhiteSpace(title)) return false;
			var normalized = title.Trim();
			return await _context.Programs.AnyAsync(p => p.Title != null && p.Title.ToLower() == normalized.ToLower());
		}

		public async Task<int> GetWeeksCount(int programId)
		{
			return await _context.ProgramWeeks.CountAsync(w => w.ProgramId == programId);
        }

		public async Task<int> GetTotalExercisesCount(int programId)
		{
			// get total exercises count in all weeks and days of the program
			return await _context.ProgramDays
				.SelectMany(d => d.Exercises)
				.CountAsync();
        }
    }
}
