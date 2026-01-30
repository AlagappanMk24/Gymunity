using Gymunity.Application.DTOs.ClientDto;

namespace Gymunity.Application.Contracts.Services.Client
{
    public interface IBodyStateLogService
    {
        Task<BodyStateLogResponse> AddAsync(string userId, CreateBodyStateLogRequest request);
        Task<List<BodyStateLogResponse>> GetStateLogsByClientAsync(string userId);
        Task<BodyStateLogResponse> GetLastStateLog(string userId);
    }
}