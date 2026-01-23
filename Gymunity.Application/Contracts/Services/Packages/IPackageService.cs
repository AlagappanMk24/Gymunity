using Gymunity.Application.DTOs.Packages;
using Gymunity.Application.DTOs.Trainers;

namespace Gymunity.Application.Contracts.Services.Packages
{
    public interface IPackageService
    {
        Task<IReadOnlyList<PackageResponse>> GetAllForTrainerAsync(int trainerId);
        Task<PackageResponse?> GetByIdAsync(int id);
        Task<PackageResponse> CreateAsync(int trainerId, PackageCreateRequest request);
        Task<PackageResponse> CreateAsyncV2(int trainerId, PackageCreateRequestV2 request);
        Task<bool> UpdateAsync(int id, PackageCreateRequest request);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
        Task<IReadOnlyList<PackageResponse>> GetAllAsync();

        // Added: trainer-related business logic moved into PackageService
        Task<IEnumerable<PackageWithSubscriptionsResponse>> GetPackagesWithSubscriptionsForTrainerAsync(int trainerProfileId);
        Task<IEnumerable<PackageWithProfitResponse>> GetPackagesWithProfitForTrainerAsync(int trainerProfileId);
    }
}