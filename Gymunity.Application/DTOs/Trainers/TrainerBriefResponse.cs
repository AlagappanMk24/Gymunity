namespace Gymunity.Application.DTOs.Trainers
{
    public class TrainerBriefResponse
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? ProfilePhotoUrl { get; set; }
        public int TrainerProfileId { get; set; }
        public string Handle { get; set; } = null!;
    }
}