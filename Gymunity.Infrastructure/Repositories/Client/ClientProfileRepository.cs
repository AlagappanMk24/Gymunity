using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Interfaces.Client;
using Gymunity.Infrastructure.Data.Context;

namespace Gymunity.Infrastructure.Repositories.Client
{
    internal class ClientProfileRepository(AppDbContext dbContext) :
       Repository<ClientProfile>(dbContext), IClientProfileRepository
    {
        //public async Task<bool> IsClientProfileCompletedAsync(string userId)
        //{
        //    var clientProfile = await _Context.ClientProfiles.FirstOrDefaultAsync(cp => cp.UserId == userId);

        //    if (clientProfile == null)
        //        return false;

        //    bool isCompleted = 
        //        clientProfile.HeightCm.HasValue &&
        //        clientProfile.StartingWeightKg.HasValue &&
        //        clientProfile.Gender != null &&
        //        clientProfile.Goal != null &&
        //        clientProfile.ExperienceLevel != null;

        //    return isCompleted;
        //}
        //public void UpdateClientPhotoAsync(AppUser user, string photoUrl)
        //{
        //    user.ProfilePhotoUrl = photoUrl;
        //    _Context.Users.Update(user);
        //}
        //public async Task<ClientProfile?> GetByUserIdAsync(string userId)      
        //    => await _Context.ClientProfiles.FirstOrDefaultAsync(cp => cp.UserId == userId);

    }
}
