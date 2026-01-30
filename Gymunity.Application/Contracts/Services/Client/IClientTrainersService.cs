using Gymunity.Application.DTOs.Trainers;

namespace Gymunity.Application.Contracts.Services.Client
{
    public interface IClientTrainersService
    {
        Task<IEnumerable<TrainerBriefResponse>> GetClientTrainers(string userId);
    }
}