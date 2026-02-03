using Gymunity.Application.DTOs.Client;
using ITI.Gymunity.FP.Application.DTOs.Client;
namespace Gymunity.Application.Contracts.Services
{
    public interface IHomeClientService
    {
        Task<(IReadOnlyList<ProgramClientResponse> programs, IReadOnlyList<TrainerClientResponse> trainers)> SearchAsync(string term);
        Task<IReadOnlyList<ProgramClientResponse>> GetAllProgramsAsync();
        Task<ProgramClientResponse?> GetProgramByIdAsync(int id);
        Task<IReadOnlyList<TrainerClientResponse>> GetAllTrainersAsync();
        Task<TrainerClientResponse?> GetTrainerByIdAsync(int id);

        // Package client endpoints
        Task<IReadOnlyList<PackageClientResponse>> GetAllPackagesAsync();
        Task<PackageClientResponse?> GetPackageByIdAsync(int id);
        Task<IReadOnlyList<PackageClientResponse>> GetPackagesByTrainerIdAsync(int trainerProfileId);

        // Program client endpoints
        Task<IReadOnlyList<ProgramClientResponse>> GetProgramsByTrainerIdAsync(string trainerId);
        Task<IReadOnlyList<PackageClientResponse>> GetPackagesByTrainerAsync(int trainerUserId);
        Task<IReadOnlyList<ProgramClientResponse>> GetProgramsByTrainerProfileIdAsync(int trainerProfileId);
    }
}
