using Gymunity.Domain.Entities.Identity;

namespace Gymunity.Admin.MVC.Services.Interfaces
{
    public interface IAdminUserResolverService
    {
        Task<AppUser?> GetPrimaryAdminAsync();
        Task<IEnumerable<AppUser>> GetAllAdminsAsync();
        Task<AppUser?> GetAdminByIdAsync(string adminUserId);
    }
}
