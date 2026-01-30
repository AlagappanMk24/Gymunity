using Gymunity.Application.DTOs.ClientDto;

namespace Gymunity.Application.Contracts.Client
{
    public interface IClientProfileService
    {
        Task<ClientProfileResponse?> GetClientProfileAsync(string userId);
        Task<ClientProfileDashboardResponse> GetDashboardAsync(string userId);
        Task<ClientProfileResponse?> CreateClientProfileAsync(string userId, ClientProfileRequest request);
        Task<ClientProfileResponse?> UpdateClientProfileAsync(string userId, ClientProfileRequest request);
        Task<bool> DeleteProfileAsync(string userId);
    }
}
