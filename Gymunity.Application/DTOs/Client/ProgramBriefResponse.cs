using Gymunity.Domain.Enums;

namespace Gymunity.Application.DTOs.Client
{
    public class ProgramBriefResponse
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public ProgramType Type { get; set; }
        public int DurationWeeks { get; set; }
    }
}
