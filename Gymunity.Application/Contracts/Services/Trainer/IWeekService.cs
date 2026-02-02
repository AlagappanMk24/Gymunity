using Gymunity.Application.DTOs.Program;

namespace Gymunity.Application.Contracts.Services.Trainer
{
    public interface IWeekService
    {
        Task<IReadOnlyList<ProgramWeekGetAllResponse>> GetByProgramAsync(int programId);
        Task<ProgramWeekGetAllResponse?> GetByIdAsync(int id);
        Task<ProgramWeekGetAllResponse> CreateAsync(ProgramWeekGetAllResponse request);
        Task<bool> UpdateAsync(int id, ProgramWeekGetAllResponse request);
        Task<bool> DeleteAsync(int id);
    }
}