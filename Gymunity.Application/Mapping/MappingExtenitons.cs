using Gymunity.Application.DTOs.Account;
using Gymunity.Domain.Entities.Identity;

namespace Gymunity.Application.Mapping
{
    public static class MappingExtenitons
    {
        public static AuthResponse ToUserResponse(this AppUser user, string token, string profilePhotoUrl)
        {
            return new AuthResponse
            {
                Id = user.Id, //amr edit i need it to test some end points
                Name = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role.ToString(),
                ProfilePhotoUrl = profilePhotoUrl,
                Token = token,
            };
        }
    }
}