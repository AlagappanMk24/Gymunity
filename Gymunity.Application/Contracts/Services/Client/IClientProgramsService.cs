using Gymunity.Application.DTOs.Program;
using Gymunity.Application.DTOs.Program.ProgramDayDtos;

namespace Gymunity.Application.Contracts.Services.Client
{
    public interface IClientProgramsService
    {
        Task<IEnumerable<ProgramResponse>> GetUserActiveProgramsAsync(string userId);
        Task<ProgramResponse?> GetProgramByIdAsync(string userId, int programId);
        Task<IEnumerable<ProgramWeekResponse>> GetAllWeeks(string userId, int programId);
        Task<IEnumerable<ProgramDayResponse>> GetAllDays(string userId, int weekId);
        Task<ProgramDayResponse?> GetDayByIdAsync(string userId, int dayId);
    }
}
