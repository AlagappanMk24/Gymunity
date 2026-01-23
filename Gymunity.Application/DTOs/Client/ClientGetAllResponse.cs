namespace Gymunity.Application.DTOs.Client
{
    public class ClientGetAllResponse
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? ProfilePhotoUrl { get; set; }
        public string? Role { get; set; }
    }
}
