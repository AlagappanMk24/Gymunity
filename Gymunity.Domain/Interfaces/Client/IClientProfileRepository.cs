using Gymunity.Domain.Entities.Client;

namespace Gymunity.Domain.Interfaces.Client
{
    public interface IClientProfileRepository: IRepository<ClientProfile>
    {
        //Task<ClientProfile?> GetByUserIdAsync(string userId);
        //public void UpdateClientPhotoAsync(AppUser user, string photoUrl);
        //Task<bool> IsClientProfileCompletedAsync(string userId);
    }
}