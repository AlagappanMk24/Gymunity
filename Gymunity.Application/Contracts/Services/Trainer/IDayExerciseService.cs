using Gymunity.Application.DTOs.Program;

namespace Gymunity.Application.Contracts.Services.Trainer
{
    public interface IDayExerciseService
    {
        Task<IReadOnlyList<ProgramDayExerciseGetAllResponse>> GetByDayAsync(int dayId);
        Task<ProgramDayExerciseGetAllResponse?> GetByIdAsync(int id);
        Task<ProgramDayExerciseGetAllResponse> CreateAsync(ProgramDayExerciseGetAllResponse request);
        Task<bool> UpdateAsync(int id, ProgramDayExerciseGetAllResponse request);
        Task<bool> DeleteAsync(int id);
    }
}