using Gymunity.Application.DTOs.Client;

namespace Gymunity.Application.Contracts.Services.Trainer
{
    public interface IClientService
    {
        Task<IReadOnlyList<ClientGetAllResponse>> GetAllByTrainerIdAsync(string trainerId);
        Task<ClientGetByIdResponse?> GetByIdAsync(string userId);
    }
}