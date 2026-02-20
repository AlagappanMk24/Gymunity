namespace Gymunity.Application.DTOs
{
    public class CachedRegistrationData
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public byte Role { get; set; }
        public byte[]? ProfilePhotoBytes { get; set; }
        public string? ProfilePhotoFileName { get; set; }
        public string? ProfilePhotoContentType { get; set; }
    }
}