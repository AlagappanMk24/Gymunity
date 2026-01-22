namespace Gymunity.Application.DTOs.ClientDto
{
    /// <summary>
    /// Client list item DTO for admin list view
    /// </summary>
    public class ClientListItemDto
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}