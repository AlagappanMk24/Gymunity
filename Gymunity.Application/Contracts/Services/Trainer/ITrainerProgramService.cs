using Gymunity.Application.DTOs.Program;

namespace Gymunity.Application.Contracts.Services.Trainer
{
    public interface ITrainerProgramService
    {
        Task<IReadOnlyList<ProgramGetAllResponse>> GetAllAsync();
        Task<ProgramGetByIdResponse?> GetByIdAsync(int id);
        Task<ProgramGetByIdResponse> CreateAsync(ProgramCreateRequest request);
        Task<bool> UpdateAsync(int id, ProgramUpdateRequest request);
        Task<bool> DeleteAsync(int id);
        Task<IReadOnlyList<ProgramGetAllResponse>> SearchAsync(string? term);
        Task<IReadOnlyList<ProgramGetAllResponse>> GetByTrainerAsync(string trainerId);
    }
}
