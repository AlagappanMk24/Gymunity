using Gymunity.Domain.Enums;

namespace Gymunity.Application.DTOs.Program
{
    public class ProgramGetByIdResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public ProgramType Type { get; set; }
        public int DurationWeeks { get; set; }
        public decimal? Price { get; set; }
        public bool IsPublic { get; set; }
        public int? MaxClients { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public int? TrainerProfileId { get; set; }
        public string? TrainerUserName { get; set; }
        public string? TrainerHandle { get; set; }
        public string? TrainerEmail { get; set; }
        public ICollection<ProgramWeekGetAllResponse> Weeks { get; set; } = [];
    }
}