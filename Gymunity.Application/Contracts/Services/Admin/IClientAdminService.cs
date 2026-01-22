using Gymunity.Application.DTOs.ClientDto;
using Gymunity.Application.Services.Admin;
using Gymunity.Application.Specifications.Admin;

namespace Gymunity.Application.Contracts.Services.Admin
{
    public interface IClientAdminService
    {
        Task<IEnumerable<ClientListItemDto>> GetAllClientsAsync(ClientFilterSpecs specs);
        Task<ClientDetailedResponse?> GetClientByIdAsync(string clientId);
        Task<int> GetClientCountAsync(ClientFilterSpecs specs);
        Task<int> GetTotalClientCountAsync();
        Task<IEnumerable<ClientListItemDto>> GetActiveClientsAsync(int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<ClientListItemDto>> GetInactiveClientsAsync(int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<ClientListItemDto>> SearchClientsAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
        Task<bool> SuspendClientAsync(int clientId);
        Task<bool> ReactivateClientAsync(int clientId);
    }
}