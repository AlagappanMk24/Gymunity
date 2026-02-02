using Gymunity.Application.DTOs.Program;

namespace Gymunity.Application.Contracts.Services.Trainer
{
    public interface IDayService
    {
        Task<IReadOnlyList<ProgramDayGetAllResponse>> GetByWeekAsync(int weekId);
        Task<ProgramDayGetAllResponse?> GetByIdAsync(int id);
        Task<ProgramDayGetAllResponse> CreateAsync(ProgramDayGetAllResponse request);
        Task<bool> UpdateAsync(int id, ProgramDayGetAllResponse request);
        Task<bool> DeleteAsync(int id);
    }
}