using Gymunity.Application.DTOs.ExerciseLibrary;

namespace Gymunity.Application.Contracts.Services.Trainer
{
    public interface IExerciseLibraryService
    {
        Task<IReadOnlyList<ExerciseGetAllResponse>> GetAllAsync(string? trainerId = null);
        Task<ExerciseGetByIdResponse?> GetByIdAsync(int id);
        Task<ExerciseGetByIdResponse> CreateAsync(ExerciseCreateRequest request);
        Task<bool> UpdateAsync(int id, ExerciseUpdateRequest request);
        Task<bool> DeleteAsync(int id);
        Task<IReadOnlyList<ExerciseGetAllResponse>> SearchByNameAsync(string? name, string? trainerId = null);
    }
}