using Gymunity.Application.DTOs.Program;
using Gymunity.Application.Services.Admin;
using Gymunity.Application.Specifications.Admin;

namespace Gymunity.Application.Contracts.Services.Admin
{
    public interface IProgramAdminService
    {
        Task<IEnumerable<ProgramGetAllResponse>> GetAllProgramsAsync(
            ProgramFilterSpecs specs);
        Task<ProgramGetByIdResponse?> GetProgramByIdAsync(int programId);
        Task<ProgramGetByIdResponse?> GetProgramDetailsWithTrainerAsync(int programId);
        Task<int> GetProgramCountAsync(ProgramFilterSpecs specs);
        Task<IEnumerable<ProgramGetAllResponse>> GetPublicProgramsAsync(
           int pageNumber = 1,
           int pageSize = 10);
        Task<IEnumerable<ProgramGetAllResponse>> GetPrivateProgramsAsync(
            int pageNumber = 1,
            int pageSize = 10);
        Task<IEnumerable<ProgramGetAllResponse>> GetProgramsByTrainerAsync(
            int trainerProfileId,
            int pageNumber = 1,
            int pageSize = 10);
        Task<IEnumerable<ProgramGetAllResponse>> SearchProgramsAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10);
        Task<int> GetPublicProgramCountAsync();
        Task<int> GetPrivateProgramCountAsync();
        Task<int> GetTrainerProgramCountAsync(int trainerProfileId);
        Task<bool> UpdateProgramAsync(int programId, ProgramUpdateRequest request);
        Task<bool> DeleteProgramAsync(int programId, bool softDelete = true);
        Task<bool> ToggleProgramVisibilityAsync(int programId);
        Task<ProgramStatsDto> GetProgramStatsAsync(int programId);
        Task<ProgramsSummaryDto> GetProgramsSummaryAsync();
    }
}
